using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Domain.Listings;

public sealed record PriceChangedPayload(Money? Before, Money? After, decimal? DeltaAmount, decimal? DeltaPercent);

public sealed record DescriptionChangedPayload(string? Before, string? After);

public sealed record PhotoChangedPayload(Uri Url, int Position, Uri? ReplacedUrl);

public sealed record FeaturesChangedPayload(IReadOnlyDictionary<string, string?> Before, IReadOnlyDictionary<string, string?> After);

public sealed record StatusChangedPayload(ListingStatus Before, ListingStatus After, PossessionStatus PossessionBefore, PossessionStatus PossessionAfter);

public sealed record RelistedPayload(DateTimeOffset LastSeenBefore, Money? PriceBefore, Money? PriceAfter);

public sealed record CreatedPayload(Money? Price, string? Municipality, PropertyType PropertyType);

public sealed record AgencyChangedPayload(Guid? Before, Guid? After);
