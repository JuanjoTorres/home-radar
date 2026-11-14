using HomeRadar.Domain.Listings;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Domain.Profiles;

public sealed record ListingFilter
{
    public decimal? PriceMin { get; init; }

    public decimal? PriceMax { get; init; }

    public decimal? PricePerM2Min { get; init; }

    public decimal? PricePerM2Max { get; init; }

    public decimal? BuiltM2Min { get; init; }

    public decimal? BuiltM2Max { get; init; }

    public int? RoomsMin { get; init; }

    public int? RoomsMax { get; init; }

    public int? BathroomsMin { get; init; }

    public IReadOnlyList<PropertyType> PropertyTypes { get; init; } = [];

    public IReadOnlyList<string> Municipalities { get; init; } = [];

    public string? PolygonWkt { get; init; }

    public GeoRadius? Radius { get; init; }

    public IReadOnlyList<SellerType> SellerTypes { get; init; } = [];

    public IReadOnlyList<string> SourceKeys { get; init; } = [];

    public IReadOnlyList<Guid> AgencyIds { get; init; } = [];

    public int? YearBuiltMin { get; init; }

    public int? YearBuiltMax { get; init; }

    public int? DaysOnMarketMin { get; init; }

    public int? DaysOnMarketMax { get; init; }

    public int? PriceDropsMin { get; init; }

    public decimal? CumulativeDropPercentMin { get; init; }

    public int? NewWithinHours { get; init; }

    public IReadOnlyList<ListingStatus> Statuses { get; init; } = [ListingStatus.Active, ListingStatus.Relisted];

    public IReadOnlyList<PossessionStatus> ExcludePossession { get; init; } = [];

    public bool? Elevator { get; init; }

    public bool? Terrace { get; init; }

    public bool? Parking { get; init; }

    public bool IncludeNewBuild { get; init; } = true;
}

public sealed record GeoRadius(double Latitude, double Longitude, int Metres);
