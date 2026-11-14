using HomeRadar.Domain.Listings;

namespace HomeRadar.Application.Notifications;

public static class OutboxMessageTypes
{
    public const string ListingMatched = "listing.matched";
    public const string WatchlistChanged = "watchlist.changed";
    public const string ListingEvent = "listing.event";
    public const string SourceHealth = "source.health";
}

public sealed record ListingMatchedMessage(Guid ListingId, string SourceKey, IReadOnlyList<Guid> SearchProfileIds);

public sealed record WatchlistChangedMessage(Guid ListingId, IReadOnlyList<Guid> EventIds);

public sealed record ListingEventMessage(Guid ListingId, Guid EventId, ListingEventType EventType);

public sealed record SourceHealthMessage(Guid SourceId, string SourceKey, string Status, string Detail);
