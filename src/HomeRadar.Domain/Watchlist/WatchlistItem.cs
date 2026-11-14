using HomeRadar.Domain.Common;

namespace HomeRadar.Domain.Watchlist;

public sealed class WatchlistItem : Entity
{
    private WatchlistItem()
    {
    }

    public WatchlistItem(Guid listingId, string? notes, DateTimeOffset now)
    {
        ListingId = listingId;
        Notes = notes;
        AddedAt = now;
        UpdatedAt = now;
    }

    public Guid ListingId { get; private init; }

    public WatchlistStatus Status { get; private set; } = WatchlistStatus.Seen;

    public string? Notes { get; private set; }

    public DateTimeOffset AddedAt { get; private init; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public uint Version { get; private set; }

    public void Update(WatchlistStatus? status, string? notes, DateTimeOffset now)
    {
        Status = status ?? Status;
        Notes = notes ?? Notes;
        UpdatedAt = now;
    }
}

public enum WatchlistStatus
{
    Seen = 0,
    Visited = 1,
    OfferMade = 2,
    Discarded = 3,
}
