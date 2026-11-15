namespace HomeRadar.Application.Search;

public sealed record AffordablePriceLimits(IReadOnlyList<SubdivisionPriceLimit> Subdivisions);

public sealed record SubdivisionPriceLimit(
    string Code,
    bool IsDefault,
    IReadOnlyList<string> MunicipalityKeys,
    decimal? ResaleMaximum,
    decimal? NewBuildMaximum);
