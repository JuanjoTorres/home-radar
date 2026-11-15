using HomeRadar.Domain.Common;

namespace HomeRadar.Domain.Intelligence;

public sealed class ListingRenovationOverride : Entity
{
    private List<RenovationOverrideItem> _items = [];

    private ListingRenovationOverride()
    {
    }

    public ListingRenovationOverride(Guid listingId, RenovationLevel? level, IEnumerable<RenovationOverrideItem> items, decimal? totalBudget, string? notes, DateTimeOffset now)
    {
        ListingId = listingId;
        Update(level, items, totalBudget, notes, now);
    }

    public Guid ListingId { get; private init; }

    public RenovationLevel? Level { get; private set; }

    public IReadOnlyList<RenovationOverrideItem> Items => _items;

    public decimal? TotalBudget { get; private set; }

    public string? Notes { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public uint Version { get; private set; }

    public void Update(RenovationLevel? level, IEnumerable<RenovationOverrideItem> items, decimal? totalBudget, string? notes, DateTimeOffset now)
    {
        _items = items.ToList();
        if (level is null && _items.Count == 0 && totalBudget is null)
        {
            throw new DomainException("A renovation override needs a level, items or a total budget.");
        }

        Level = level;
        TotalBudget = totalBudget;
        Notes = notes;
        UpdatedAt = now;
    }

    public RenovationOverride ToOverride() => new(Level, _items, TotalBudget, Notes, UpdatedAt);
}
