using HomeRadar.Domain.Finance;

namespace HomeRadar.Domain.Intelligence;

public static class CadastralMatcher
{
    private static readonly string[] ResidentialUses = ["residencial", "vivienda", "v"];

    public static CadastralFacts Match(CadastralParcel parcel, string? unitReference, decimal? advertisedBuiltSquareMetres, decimal tolerancePercent)
    {
        if (parcel.Status == CadastralLookupStatus.NotFound || parcel.Units.Count == 0)
        {
            return new CadastralFacts(parcel.ParcelReference, null, null, null, parcel.IsRustic, CadastralMatchQuality.None);
        }

        if (unitReference is not null && parcel.Units.FirstOrDefault(unit => string.Equals(unit.Reference, unitReference, StringComparison.OrdinalIgnoreCase)) is { } exact)
        {
            return Facts(exact, parcel, CadastralMatchQuality.Unit);
        }

        var dwellings = parcel.Units.Where(unit => unit.Use is { } use && ResidentialUses.Contains(TextKeys.Fold(use))).ToList();
        var candidates = dwellings.Count > 0 ? dwellings : parcel.Units.ToList();
        if (candidates.Count == 1)
        {
            return Facts(candidates[0], parcel, CadastralMatchQuality.Unit);
        }

        if (advertisedBuiltSquareMetres is { } advertised && advertised > 0m)
        {
            var ranked = candidates
                .Where(unit => unit.BuiltSquareMetres is > 0m)
                .Select(unit => (Unit: unit, Gap: Math.Abs(unit.BuiltSquareMetres!.Value - advertised) / advertised * 100m))
                .OrderBy(candidate => candidate.Gap)
                .ToList();
            if (ranked.Count > 0 && ranked[0].Gap <= tolerancePercent && (ranked.Count == 1 || ranked[1].Gap - ranked[0].Gap >= 5m))
            {
                return Facts(ranked[0].Unit, parcel, CadastralMatchQuality.Unit);
            }
        }

        var year = candidates.Where(unit => unit.YearBuilt is not null).GroupBy(unit => unit.YearBuilt).OrderByDescending(group => group.Count()).ThenBy(group => group.Key).Select(group => group.Key).FirstOrDefault();
        return new CadastralFacts(parcel.ParcelReference, null, year, candidates[0].Use, parcel.IsRustic, CadastralMatchQuality.Parcel);
    }

    private static CadastralFacts Facts(CadastralUnit unit, CadastralParcel parcel, CadastralMatchQuality quality) =>
        new(unit.Reference, unit.BuiltSquareMetres, unit.YearBuilt, unit.Use, parcel.IsRustic, quality);
}
