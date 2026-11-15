namespace HomeRadar.Application.Search;

public sealed record AffordablePriceLimits(IReadOnlyList<SubdivisionPriceLimit> Subdivisions, Func<HomeRadar.Domain.Listings.Listing, bool> Accepts);

public sealed record SubdivisionPriceLimit(
    string Code,
    bool IsDefault,
    IReadOnlyList<string> MunicipalityKeys,
    decimal? ResaleMaximum,
    decimal? NewBuildMaximum);
