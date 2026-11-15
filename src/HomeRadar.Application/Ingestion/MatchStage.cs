using HomeRadar.Application.Analysis;
using HomeRadar.Application.Ports;
using HomeRadar.Application.Search;
using HomeRadar.Domain.Common;
using HomeRadar.Domain.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeRadar.Application.Ingestion;

public sealed partial class MatchStage(IHomeRadarDbContext db, AffordabilityLimitResolver affordability, ILogger<MatchStage> logger) : IIngestionStage
{
    private readonly Dictionary<Guid, Result<AffordablePriceLimits>> _limits = [];

    public int Order => StageOrder.Match;

    public async Task ExecuteAsync(IngestionContext context, CancellationToken cancellationToken)
    {
        if (!context.ShouldContinue)
        {
            return;
        }

        var listing = context.Listing!;
        context.OnWatchlist = await db.WatchlistItems.AsNoTracking().AnyAsync(item => item.ListingId == listing.Id, cancellationToken);

        if (!context.IsNew)
        {
            return;
        }

        var profiles = await db.SearchProfiles.AsNoTracking().Where(profile => profile.Enabled).ToListAsync(cancellationToken);
        foreach (var profile in profiles)
        {
            AffordablePriceLimits? limits = null;
            if (profile.Filter.Affordability is { } criteria)
            {
                var resolved = await LimitsAsync(profile, criteria, cancellationToken);
                if (!resolved.IsSuccess)
                {
                    AffordabilityUnavailable(logger, profile.Key, resolved.Error!.Code);
                    continue;
                }

                limits = resolved.Value;
            }

            var predicate = ListingFilterSpecification.Build(profile.Filter, context.Now, limits).Compile();
            if (predicate(listing))
            {
                context.MatchedSearchProfileIds.Add(profile.Id);
            }
        }
    }

    private async Task<Result<AffordablePriceLimits>> LimitsAsync(SearchProfile profile, AffordabilityCriteria criteria, CancellationToken cancellationToken)
    {
        if (!_limits.TryGetValue(profile.Id, out var limits))
        {
            limits = await affordability.ResolveAsync(criteria, cancellationToken);
            _limits[profile.Id] = limits;
        }

        return limits;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Search profile {Key} skipped: affordability limits unavailable ({Code})")]
    private static partial void AffordabilityUnavailable(ILogger logger, string key, string code);
}
