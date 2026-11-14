using HomeRadar.Application.Ports;
using HomeRadar.Domain.Listings;
using Microsoft.EntityFrameworkCore;

namespace HomeRadar.Application.Ingestion;

public sealed class ChangeDetectionStage(IHomeRadarDbContext db) : IIngestionStage
{
    public int Order => StageOrder.DetectChanges;

    public async Task ExecuteAsync(IngestionContext context, CancellationToken cancellationToken)
    {
        var observation = context.Fetched.Observation;
        var listing = await db.Listings
            .Include(candidate => candidate.Photos)
            .SingleOrDefaultAsync(candidate => candidate.SourceId == context.Source.Id && candidate.ExternalId == observation.ExternalId, cancellationToken);

        context.Run.CountSeen();

        if (listing is null)
        {
            listing = Listing.Create(context.Source.Id, context.Source.Key, context.Source.SellerType, observation, context.Now);
            db.Listings.Add(listing);
            context.IsNew = true;
            context.Run.CountCreated();
        }
        else
        {
            var outcome = listing.Observe(observation, context.Now);
            context.Changed = outcome == ObservationOutcome.Changed;
            if (context.Changed)
            {
                context.Run.CountChanged();
            }
        }

        context.Listing = listing;
        context.NewEvents.AddRange(listing.DomainEvents.OfType<ListingEvent>());

        if (context.IsNew || context.Changed)
        {
            var snapshot = new ListingSnapshot(
                listing.Id,
                context.Run.Id,
                context.Now,
                listing.ContentHash,
                context.Fetched.Raw.ContentType,
                SnapshotCompression.Compress(context.Fetched.Raw.Body));
            db.ListingSnapshots.Add(snapshot);
            foreach (var listingEvent in context.NewEvents)
            {
                listingEvent.AttachSnapshot(snapshot.Id);
            }
        }
    }
}
