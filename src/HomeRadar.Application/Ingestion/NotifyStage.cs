using System.Text.Json;
using HomeRadar.Application.Notifications;
using HomeRadar.Application.Ports;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.Outbox;

namespace HomeRadar.Application.Ingestion;

public sealed class NotifyStage(IHomeRadarDbContext db) : IIngestionStage
{
    public int Order => StageOrder.Notify;

    public Task ExecuteAsync(IngestionContext context, CancellationToken cancellationToken)
    {
        if (!context.ShouldContinue)
        {
            return Task.CompletedTask;
        }

        var listing = context.Listing!;
        if (context.IsNew && context.MatchedSearchProfileIds.Count > 0)
        {
            Enqueue(OutboxMessageTypes.ListingMatched, new ListingMatchedMessage(listing.Id, context.Source.Key, context.MatchedSearchProfileIds), context.Now);
        }

        if (!context.IsNew && context.OnWatchlist && context.NewEvents.Count > 0)
        {
            Enqueue(OutboxMessageTypes.WatchlistChanged, new WatchlistChangedMessage(listing.Id, context.NewEvents.Select(e => e.Id).ToList()), context.Now);
        }

        foreach (var listingEvent in context.NewEvents.Where(e => e.Type is ListingEventType.Created or ListingEventType.PriceChanged or ListingEventType.StatusChanged or ListingEventType.Relisted))
        {
            Enqueue(OutboxMessageTypes.ListingEvent, new ListingEventMessage(listing.Id, listingEvent.Id, listingEvent.Type), context.Now);
        }

        return Task.CompletedTask;
    }

    private void Enqueue(string type, object payload, DateTimeOffset now) =>
        db.OutboxMessages.Add(new OutboxMessage(type, JsonSerializer.Serialize(payload, PayloadSerialization.Options), now));
}
