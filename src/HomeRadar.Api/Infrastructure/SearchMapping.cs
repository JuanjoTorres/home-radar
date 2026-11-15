using HomeRadar.Api.Contracts;
using HomeRadar.Domain.Finance;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Api.Infrastructure;

public static class SearchMapping
{
    public static ListingFilter ToFilter(ListingSearchRequestDto request) => new()
    {
        PriceMin = request.Price?.Min,
        PriceMax = request.Price?.Max,
        PricePerM2Min = request.PricePerM2?.Min,
        PricePerM2Max = request.PricePerM2?.Max,
        BuiltM2Min = request.BuiltM2?.Min,
        BuiltM2Max = request.BuiltM2?.Max,
        RoomsMin = request.Rooms?.Min,
        RoomsMax = request.Rooms?.Max,
        BathroomsMin = request.BathroomsMin,
        PropertyTypes = ParseAll<PropertyType>(request.PropertyTypes),
        Municipalities = request.Municipalities ?? [],
        PolygonWkt = request.PolygonWkt,
        Radius = request.Radius is { } radius ? new GeoRadius(radius.Latitude, radius.Longitude, radius.Metres) : null,
        SellerTypes = ParseAll<SellerType>(request.SellerTypes),
        SourceKeys = request.SourceKeys ?? [],
        YearBuiltMin = request.YearBuilt?.Min,
        YearBuiltMax = request.YearBuilt?.Max,
        DaysOnMarketMin = request.DaysOnMarket?.Min,
        DaysOnMarketMax = request.DaysOnMarket?.Max,
        PriceDropsMin = request.PriceDropsMin,
        CumulativeDropPercentMin = request.CumulativeDropPercentMin,
        NewWithinHours = request.NewWithinHours,
        Statuses = request.Statuses is { Count: > 0 } ? ParseAll<ListingStatus>(request.Statuses) : [ListingStatus.Active, ListingStatus.Relisted],
        ExcludePossession = ParseAll<PossessionStatus>(request.ExcludePossession),
        Elevator = request.Elevator,
        Terrace = request.Terrace,
        Parking = request.Parking,
        IncludeNewBuild = request.IncludeNewBuild,
        Affordability = request.Affordability is { } affordability
            ? new AffordabilityCriteria
            {
                FinancingProfileId = affordability.FinancingProfileId,
                Budget = affordability.Budget?.Amount,
                MinimumVerdict = Enum.Parse<AffordabilityVerdict>(affordability.MinimumVerdict, ignoreCase: true),
                IncludeGuarantees = affordability.IncludeGuarantees,
            }
            : null,
    };

    public static ListingSearchRequestDto ToRequest(ListingFilter filter) => new()
    {
        Price = Range(filter.PriceMin, filter.PriceMax),
        PricePerM2 = Range(filter.PricePerM2Min, filter.PricePerM2Max),
        BuiltM2 = Range(filter.BuiltM2Min, filter.BuiltM2Max),
        Rooms = Range(filter.RoomsMin, filter.RoomsMax),
        BathroomsMin = filter.BathroomsMin,
        PropertyTypes = filter.PropertyTypes.Select(type => type.ToString()).ToList(),
        Municipalities = filter.Municipalities,
        PolygonWkt = filter.PolygonWkt,
        Radius = filter.Radius is { } radius ? new RadiusDto(radius.Latitude, radius.Longitude, radius.Metres) : null,
        SellerTypes = filter.SellerTypes.Select(type => type.ToString()).ToList(),
        SourceKeys = filter.SourceKeys,
        YearBuilt = Range(filter.YearBuiltMin, filter.YearBuiltMax),
        DaysOnMarket = Range(filter.DaysOnMarketMin, filter.DaysOnMarketMax),
        PriceDropsMin = filter.PriceDropsMin,
        CumulativeDropPercentMin = filter.CumulativeDropPercentMin,
        NewWithinHours = filter.NewWithinHours,
        Statuses = filter.Statuses.Select(status => status.ToString()).ToList(),
        ExcludePossession = filter.ExcludePossession.Select(status => status.ToString()).ToList(),
        Elevator = filter.Elevator,
        Terrace = filter.Terrace,
        Parking = filter.Parking,
        IncludeNewBuild = filter.IncludeNewBuild,
        Affordability = filter.Affordability is { } affordability
            ? new AffordabilityFilterDto
            {
                FinancingProfileId = affordability.FinancingProfileId,
                Budget = affordability.Budget is { } budget ? new MoneyDto(budget) : null,
                MinimumVerdict = affordability.MinimumVerdict.ToString(),
                IncludeGuarantees = affordability.IncludeGuarantees,
            }
            : null,
    };

    private static List<T> ParseAll<T>(IReadOnlyList<string>? values)
        where T : struct, Enum =>
        values is null ? [] : values.Select(value => Enum.Parse<T>(value, ignoreCase: true)).ToList();

    private static DecimalRangeDto? Range(decimal? min, decimal? max) => min is null && max is null ? null : new DecimalRangeDto(min, max);

    private static IntRangeDto? Range(int? min, int? max) => min is null && max is null ? null : new IntRangeDto(min, max);
}
