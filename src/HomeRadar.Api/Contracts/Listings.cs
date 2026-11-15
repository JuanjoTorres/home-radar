using HomeRadar.Domain.Listings;

namespace HomeRadar.Api.Contracts;

/// <summary>Sort orders for listing searches.</summary>
public enum ListingSortDto
{
    /// <summary>Most recently seen first (default).</summary>
    FirstSeenDesc,

    /// <summary>Cheapest first.</summary>
    PriceAsc,

    /// <summary>Most expensive first.</summary>
    PriceDesc,

    /// <summary>Lowest price per built square metre first.</summary>
    PricePerM2,

    /// <summary>Highest opportunity score first; listings without a score last.</summary>
    ScoreDesc,

    /// <summary>Largest cumulative price drop first.</summary>
    PriceDropDesc,
}

/// <summary>Compact listing representation used in lists and search results.</summary>
public sealed record ListingSummaryDto
{
    /// <summary>Listing identifier.</summary>
    public required Guid Id { get; init; }

    /// <summary>Physical home grouping several listings, when matched (phase 4).</summary>
    public Guid? DwellingId { get; init; }

    /// <summary>Source key, e.g. solvia.</summary>
    public required string SourceKey { get; init; }

    /// <summary>Kind of seller behind the source.</summary>
    public required string SellerType { get; init; }

    /// <summary>Identifier of the listing inside its source.</summary>
    public required string ExternalId { get; init; }

    /// <summary>Public URL of the listing.</summary>
    public required Uri Url { get; init; }

    /// <summary>Title as published.</summary>
    public required string Title { get; init; }

    /// <summary>Current asking price; <c>null</c> when the source hides it.</summary>
    public MoneyDto? Price { get; init; }

    /// <summary>Price per built square metre in EUR, when both figures are known.</summary>
    public decimal? PricePerSquareMetre { get; init; }

    /// <summary>Built area in m².</summary>
    public decimal? BuiltSquareMetres { get; init; }

    /// <summary>Usable area in m² when published.</summary>
    public decimal? UsableSquareMetres { get; init; }

    /// <summary>Number of bedrooms.</summary>
    public int? Rooms { get; init; }

    /// <summary>Number of bathrooms.</summary>
    public int? Bathrooms { get; init; }

    /// <summary>Normalised property type.</summary>
    public required string PropertyType { get; init; }

    /// <summary>Municipality, e.g. Inca.</summary>
    public string? Municipality { get; init; }

    /// <summary>Zone, street or urbanisation as published.</summary>
    public string? Zone { get; init; }

    /// <summary>Location when the source publishes coordinates.</summary>
    public GeoPointDto? Location { get; init; }

    /// <summary>Lifecycle status.</summary>
    public required string Status { get; init; }

    /// <summary>Occupancy as published by the source; critical for bank-owned stock.</summary>
    public required string Possession { get; init; }

    /// <summary>When the listing was first seen by HomeRadar (UTC).</summary>
    public required DateTimeOffset FirstSeenAt { get; init; }

    /// <summary>When the listing was last confirmed online (UTC).</summary>
    public required DateTimeOffset LastSeenAt { get; init; }

    /// <summary>Publication date according to the source, when available.</summary>
    public DateTimeOffset? PublishedAt { get; init; }

    /// <summary>Days since publication (or since first seen when the source gives no date).</summary>
    public required int DaysOnMarket { get; init; }

    /// <summary>Number of price drops recorded.</summary>
    public required int PriceDrops { get; init; }

    /// <summary>Total drop since the first price seen, in %.</summary>
    public required decimal CumulativeDropPercent { get; init; }

    /// <summary>First photo, when any.</summary>
    public Uri? MainPhotoUrl { get; init; }

    /// <summary>Opportunity score 0–100 for the default financing profile, refreshed on every change and daily.</summary>
    public int? Score { get; init; }

    /// <summary>Estimated (or manually set) renovation level: ReadyToMoveIn, Light, Medium or Full.</summary>
    public string? RenovationLevel { get; init; }

    /// <summary>Most likely renovation cost, VAT included.</summary>
    public MoneyDto? RenovationCost { get; init; }

    /// <summary>Red flag codes (see getListingAnalysis for details).</summary>
    public IReadOnlyList<string> RedFlags { get; init; } = [];

    /// <summary>Construction year: Catastro when matched, else the listing.</summary>
    public int? YearBuilt { get; init; }

    public static ListingSummaryDto From(Listing listing, DateTimeOffset now) => new()
    {
        Id = listing.Id,
        DwellingId = listing.DwellingId,
        SourceKey = listing.SourceKey,
        SellerType = listing.SellerType.ToString(),
        ExternalId = listing.ExternalId,
        Url = listing.Url,
        Title = listing.Title,
        Price = MoneyDto.From(listing.Price),
        PricePerSquareMetre = listing.PricePerSquareMetre,
        BuiltSquareMetres = listing.BuiltSquareMetres,
        UsableSquareMetres = listing.UsableSquareMetres,
        Rooms = listing.Rooms,
        Bathrooms = listing.Bathrooms,
        PropertyType = listing.PropertyType.ToString(),
        Municipality = listing.Municipality,
        Zone = listing.Zone,
        Location = GeoPointDto.From(listing.Location),
        Status = listing.Status.ToString(),
        Possession = listing.Possession.ToString(),
        FirstSeenAt = listing.FirstSeenAt,
        LastSeenAt = listing.LastSeenAt,
        PublishedAt = listing.PublishedAt,
        DaysOnMarket = Math.Max(0, (int)(now - (listing.PublishedAt ?? listing.FirstSeenAt)).TotalDays),
        PriceDrops = listing.PriceDropCount,
        CumulativeDropPercent = listing.CumulativeDropPercent,
        MainPhotoUrl = listing.Photos.OrderBy(photo => photo.Position).FirstOrDefault()?.Url,
        Score = listing.Score,
        RenovationLevel = listing.RenovationLevel?.ToString(),
        RenovationCost = listing.RenovationProbableCost is { } cost ? new MoneyDto(cost) : null,
        RedFlags = listing.RedFlagCodes,
        YearBuilt = listing.EffectiveYearBuilt,
    };
}

/// <summary>Full listing, including description, features and photos.</summary>
public sealed record ListingDetailDto
{
    /// <summary>Summary fields.</summary>
    public required ListingSummaryDto Summary { get; init; }

    /// <summary>Description as published.</summary>
    public string? Description { get; init; }

    /// <summary>Normalised features (elevator, terrace, parking, yearBuilt, energyRating, previousPrice…).</summary>
    public required IReadOnlyDictionary<string, string> Features { get; init; }

    /// <summary>Derived facts added by the rules engine, keyed by name.</summary>
    public required IReadOnlyDictionary<string, string> Enrichments { get; init; }

    /// <summary>Photos in published order.</summary>
    public required IReadOnlyList<PhotoDto> Photos { get; init; }

    /// <summary>Cadastral reference (20 characters) when the source publishes it.</summary>
    public string? CadastralReference { get; init; }

    /// <summary>Other listings of the same home, when matched (phase 4).</summary>
    public required IReadOnlyList<ListingSummaryDto> SiblingListings { get; init; }

    public static ListingDetailDto From(Listing listing, IReadOnlyList<Listing> siblings, DateTimeOffset now) => new()
    {
        Summary = ListingSummaryDto.From(listing, now),
        Description = listing.Description,
        Features = listing.Features.Where(pair => !pair.Key.StartsWith('_')).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
        Enrichments = listing.Enrichments,
        Photos = listing.Photos.OrderBy(photo => photo.Position).Select(photo => new PhotoDto(photo.Url, photo.Position, photo.PerceptualHash?.ToString("x16", System.Globalization.CultureInfo.InvariantCulture))).ToList(),
        CadastralReference = listing.CadastralReference?.Value,
        SiblingListings = siblings.Select(sibling => ListingSummaryDto.From(sibling, now)).ToList(),
    };
}

/// <summary>Photo of a listing.</summary>
/// <param name="Url">Original URL.</param>
/// <param name="Position">Zero-based position in the gallery.</param>
/// <param name="PerceptualHash">64-bit perceptual hash in hex, once computed.</param>
public sealed record PhotoDto(Uri Url, int Position, string? PerceptualHash);

/// <summary>One recorded change of a listing.</summary>
public sealed record ListingEventDto
{
    /// <summary>Event identifier.</summary>
    public required Guid Id { get; init; }

    /// <summary>Event type: Created, PriceChanged, DescriptionChanged, PhotoAdded, PhotoRemoved, PhotoReplaced, FeaturesChanged, StatusChanged, Relisted, CrossListed, AgencyChanged.</summary>
    public required string Type { get; init; }

    /// <summary>When the change was detected (UTC).</summary>
    public required DateTimeOffset OccurredAt { get; init; }

    /// <summary>Raw snapshot that evidences the change.</summary>
    public Guid? SnapshotId { get; init; }

    /// <summary>Event-specific payload. PriceChanged: before, after, deltaAmount, deltaPercent. DescriptionChanged: before, after. Photo events: url, position, replacedUrl.</summary>
    public required System.Text.Json.JsonElement Payload { get; init; }

    public static ListingEventDto From(ListingEvent listingEvent) => new()
    {
        Id = listingEvent.Id,
        Type = listingEvent.Type.ToString(),
        OccurredAt = listingEvent.OccurredAt,
        SnapshotId = listingEvent.SnapshotId,
        Payload = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(listingEvent.Payload),
    };
}

/// <summary>Search criteria. Every field is optional; omitted fields do not filter.</summary>
public sealed record ListingSearchRequestDto
{
    /// <summary>Asking price range in EUR.</summary>
    public DecimalRangeDto? Price { get; init; }

    /// <summary>Price per built m² range in EUR.</summary>
    public DecimalRangeDto? PricePerM2 { get; init; }

    /// <summary>Built area range in m².</summary>
    public DecimalRangeDto? BuiltM2 { get; init; }

    /// <summary>Bedrooms range.</summary>
    public IntRangeDto? Rooms { get; init; }

    /// <summary>Minimum bathrooms.</summary>
    public int? BathroomsMin { get; init; }

    /// <summary>Property types to include: Flat, House, Townhouse, Finca, Penthouse, Studio, Garage, Commercial, Land, Other.</summary>
    public IReadOnlyList<string>? PropertyTypes { get; init; }

    /// <summary>Municipalities (case-insensitive), e.g. ["Inca", "Lloseta"].</summary>
    public IReadOnlyList<string>? Municipalities { get; init; }

    /// <summary>WKT polygon in WGS84 (lon lat pairs).</summary>
    public string? PolygonWkt { get; init; }

    /// <summary>Circle around a point.</summary>
    public RadiusDto? Radius { get; init; }

    /// <summary>Seller types: Servicer, Bank, Agency, Auction, Private.</summary>
    public IReadOnlyList<string>? SellerTypes { get; init; }

    /// <summary>Source keys, e.g. ["solvia", "aliseda"].</summary>
    public IReadOnlyList<string>? SourceKeys { get; init; }

    /// <summary>Year built range (needs sources that publish it).</summary>
    public IntRangeDto? YearBuilt { get; init; }

    /// <summary>Days on market range.</summary>
    public IntRangeDto? DaysOnMarket { get; init; }

    /// <summary>Minimum number of price drops.</summary>
    public int? PriceDropsMin { get; init; }

    /// <summary>Minimum cumulative drop in %.</summary>
    public decimal? CumulativeDropPercentMin { get; init; }

    /// <summary>Only listings first seen within the last N hours (1–720).</summary>
    public int? NewWithinHours { get; init; }

    /// <summary>Statuses to include; defaults to Active and Relisted.</summary>
    public IReadOnlyList<string>? Statuses { get; init; }

    /// <summary>Possession statuses to exclude, e.g. ["NoPossession", "Occupied"].</summary>
    public IReadOnlyList<string>? ExcludePossession { get; init; }

    /// <summary>Require (true) or exclude (false) elevator.</summary>
    public bool? Elevator { get; init; }

    /// <summary>Require (true) or exclude (false) terrace.</summary>
    public bool? Terrace { get; init; }

    /// <summary>Require (true) or exclude (false) parking.</summary>
    public bool? Parking { get; init; }

    /// <summary>Include new-build listings; defaults to true.</summary>
    public bool IncludeNewBuild { get; init; } = true;

    /// <summary>Return only what the buyer can pay, taxes, closing costs and the estimated (or manual) renovation included. See <c>calculateMaximumAffordablePrice</c> for the calculation.</summary>
    public AffordabilityFilterDto? Affordability { get; init; }

    /// <summary>Minimum opportunity score (0–100).</summary>
    public int? MinScore { get; init; }

    /// <summary>Renovation levels to keep: ReadyToMoveIn, Light, Medium, Full.</summary>
    public IReadOnlyList<string> RenovationLevels { get; init; } = [];

    /// <summary>Red flag codes to exclude, e.g. occupied, noPossession, noHabitabilityCertificate.</summary>
    public IReadOnlyList<string> ExcludeRedFlags { get; init; } = [];

    /// <summary>Sort order; defaults to FirstSeenDesc.</summary>
    public ListingSortDto Sort { get; init; } = ListingSortDto.FirstSeenDesc;

    /// <summary>Cursor from a previous page.</summary>
    public string? Cursor { get; init; }

    /// <summary>Page size (1–100), default 25.</summary>
    public int Limit { get; init; } = 25;
}

/// <summary>Affordability filter. With a profile, a listing passes when its price is at most the profile's maximum affordable price for its island and kind (resale or new build); with a budget, when price + taxes + closing costs fit in it; with both, both limits apply.</summary>
public sealed record AffordabilityFilterDto
{
    /// <summary>Financing profile whose maximum affordable price is applied.</summary>
    public Guid? FinancingProfileId { get; init; }

    /// <summary>Total budget: price + taxes + closing costs must fit in it.</summary>
    public MoneyDto? Budget { get; init; }

    /// <summary>Affordable (default) or Borderline.</summary>
    public string MinimumVerdict { get; init; } = "Affordable";

    /// <summary>Count an eligible ICO or IBAVI guarantee when it raises the loan-to-value.</summary>
    public bool IncludeGuarantees { get; init; }
}
