using System.Text.Json;
using HomeRadar.Application.Notifications;
using HomeRadar.Application.Ports;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.Outbox;
using HomeRadar.Domain.Sources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeRadar.Application.Ingestion;

public sealed class SourceIngestionService(
    IHomeRadarDbContext db,
    ISourceAdapterRegistry adapters,
    IEnumerable<IIngestionStage> stages,
    TimeProvider clock,
    ILogger<SourceIngestionService> logger)
{
    private const decimal MaxIncompleteFraction = 0.2m;

    private readonly IReadOnlyList<IIngestionStage> _stages = stages.OrderBy(stage => stage.Order).ToList();

    public async Task<SourceRun> RunAsync(Guid sourceId, bool fullScan, CancellationToken cancellationToken)
    {
        var source = await db.Sources.SingleAsync(candidate => candidate.Id == sourceId, cancellationToken);
        var run = new SourceRun(source.Id, clock.GetUtcNow(), fullScan);
        db.SourceRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            var adapter = adapters.Resolve(source.AdapterKey);
            var incomplete = 0;
            var seenExternalIds = new HashSet<string>(StringComparer.Ordinal);

            var context = new SourceFetchContext(source, fullScan, null, (externalId, token) => StoredFingerprintAsync(source.Id, externalId, token));
            await foreach (var fetched in adapter.FetchAsync(context, cancellationToken))
            {
                if (fetched.IsUnchanged)
                {
                    seenExternalIds.Add(fetched.ExternalId);
                    await TouchAsync(source, run, fetched.ExternalId, cancellationToken);
                    continue;
                }

                if (!IsUsable(fetched.Observation!))
                {
                    incomplete++;
                    continue;
                }

                seenExternalIds.Add(fetched.ExternalId);
                await IngestOneAsync(source, run, fetched, cancellationToken);
            }

            var outcome = Evaluate(source, run, incomplete, seenExternalIds.Count);
            if (outcome == SourceRunOutcome.Succeeded && fullScan)
            {
                await WithdrawMissingAsync(source, run, seenExternalIds, cancellationToken);
            }

            Finish(source, run, outcome, outcome == SourceRunOutcome.Degraded ? DegradedMessage(run, incomplete) : null);
        }
        catch (SourceSchemaChangedException exception)
        {
            logger.LogError(exception, "Source {SourceKey} schema changed", source.Key);
            Finish(source, run, SourceRunOutcome.Failed, exception.Message);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Source {SourceKey} run failed", source.Key);
            run.CountError();
            Finish(source, run, SourceRunOutcome.Failed, exception.Message);
        }

        await db.SaveChangesAsync(cancellationToken);
        return run;
    }

    private async Task<string?> StoredFingerprintAsync(Guid sourceId, string externalId, CancellationToken cancellationToken)
    {
        var features = await db.Listings.AsNoTracking()
            .Where(listing => listing.SourceId == sourceId && listing.ExternalId == externalId)
            .Select(listing => listing.Features)
            .SingleOrDefaultAsync(cancellationToken);
        return features is not null && features.TryGetValue(FeatureKeys.Fingerprint, out var fingerprint) ? fingerprint : null;
    }

    private async Task TouchAsync(Source source, SourceRun run, string externalId, CancellationToken cancellationToken)
    {
        var listing = await db.Listings.SingleOrDefaultAsync(candidate => candidate.SourceId == source.Id && candidate.ExternalId == externalId, cancellationToken);
        if (listing is null)
        {
            return;
        }

        run.CountSeen();
        listing.Touch(clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
    }

    private static bool IsUsable(ListingObservation observation) =>
        observation.ExternalId.Length > 0 && observation.Url.IsAbsoluteUri && observation.Price is not null;

    private async Task IngestOneAsync(Source source, SourceRun run, FetchedListing fetched, CancellationToken cancellationToken)
    {
        var context = new IngestionContext(source, run, fetched, clock.GetUtcNow());
        try
        {
            foreach (var stage in _stages)
            {
                await stage.ExecuteAsync(context, cancellationToken);
            }

            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            run.CountError();
            logger.LogWarning(exception, "Listing {ExternalId} from {SourceKey} could not be ingested", fetched.ExternalId, source.Key);
        }
    }

    private static SourceRunOutcome Evaluate(Source source, SourceRun run, int incomplete, int usable)
    {
        var total = usable + incomplete;
        if (total == 0 && source.LastListingCount > 0)
        {
            return SourceRunOutcome.Degraded;
        }

        if (total > 0 && (decimal)incomplete / total > MaxIncompleteFraction)
        {
            return SourceRunOutcome.Degraded;
        }

        return run.Errors > 0 && run.Errors >= usable ? SourceRunOutcome.Failed : SourceRunOutcome.Succeeded;
    }

    private static string DegradedMessage(SourceRun run, int incomplete) =>
        $"Run saw {run.ListingsSeen} usable and {incomplete} incomplete listings; previous runs returned data. Nothing was withdrawn.";

    private async Task WithdrawMissingAsync(Source source, SourceRun run, HashSet<string> seen, CancellationToken cancellationToken)
    {
        var active = await db.Listings
            .Where(listing => listing.SourceId == source.Id && listing.Status != ListingStatus.Withdrawn && listing.Status != ListingStatus.Sold)
            .ToListAsync(cancellationToken);
        var now = clock.GetUtcNow();
        foreach (var listing in active.Where(listing => !seen.Contains(listing.ExternalId)))
        {
            listing.MarkNotSeen(now);
            run.CountWithdrawn();
            var listingEvent = listing.DomainEvents.OfType<ListingEvent>().Last();
            db.OutboxMessages.Add(new OutboxMessage(
                OutboxMessageTypes.ListingEvent,
                JsonSerializer.Serialize(new ListingEventMessage(listing.Id, listingEvent.Id, listingEvent.Type), PayloadSerialization.Options),
                now));
        }
    }

    private void Finish(Source source, SourceRun run, SourceRunOutcome outcome, string? message)
    {
        var now = clock.GetUtcNow();
        var previousStatus = source.Status;
        run.Finish(outcome, now, message);
        source.RecordRun(run);
        if (source.Status != previousStatus)
        {
            db.OutboxMessages.Add(new OutboxMessage(
                OutboxMessageTypes.SourceHealth,
                JsonSerializer.Serialize(new SourceHealthMessage(source.Id, source.Key, source.Status.ToString(), message ?? $"{run.ListingsSeen} listings, {run.Errors} errors"), PayloadSerialization.Options),
                now));
        }
    }
}
