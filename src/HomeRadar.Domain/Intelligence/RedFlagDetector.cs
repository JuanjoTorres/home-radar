using System.Globalization;
using HomeRadar.Domain.Finance;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Domain.Intelligence;

public sealed record RedFlag(string Code, RedFlagSeverity Severity, string Description, string Evidence);

public sealed record CadastralFacts(
    string? Reference,
    decimal? BuiltSquareMetres,
    int? YearBuilt,
    string? Use,
    bool? IsRustic,
    CadastralMatchQuality Match);

public enum CadastralMatchQuality
{
    None = 0,
    Parcel = 1,
    Unit = 2,
    Manual = 3,
}

public sealed record RedFlagInput(
    string? Title,
    string? Description,
    PossessionStatus Possession,
    decimal? AdvertisedBuiltSquareMetres,
    int? YearBuilt,
    CadastralFacts? Cadastre,
    int CurrentYear);

public static class RedFlagDetector
{
    public const string NoPossessionCode = "noPossession";
    public const string OccupiedCode = "occupied";
    public const string TenantCode = "tenantInPlace";
    public const string AreaMismatchHighCode = "areaMismatchHigh";
    public const string AreaMismatchCode = "areaMismatch";
    public const string AreaMismatchLowCode = "areaMismatchLow";
    public const string RusticCode = "ruralLand";
    public const string InspectionCode = "buildingOver50NoIee";

    public static IReadOnlyList<RedFlag> Detect(RedFlagInput input, RedFlagConfig config)
    {
        var flags = new List<RedFlag>();
        var words = TextKeys.Words($"{input.Title} {input.Description}");
        foreach (var rule in config.Rules)
        {
            var hit = rule.Keywords.FirstOrDefault(keyword => TextKeys.ContainsPhrase(words, keyword));
            if (hit is null || rule.Exclusions.Any(exclusion => TextKeys.ContainsPhrase(words, exclusion)))
            {
                continue;
            }

            flags.Add(new RedFlag(rule.Code, rule.Severity, rule.Description, $"Text mentions \"{hit}\""));
        }

        switch (input.Possession)
        {
            case PossessionStatus.NoPossession:
                AddOnce(flags, new RedFlag(NoPossessionCode, RedFlagSeverity.Critical, "Sold without possession: no viewing, no appraisal, no standard mortgage", "Possession status NoPossession"));
                break;
            case PossessionStatus.Occupied:
                AddOnce(flags, new RedFlag(OccupiedCode, RedFlagSeverity.Critical, "Occupied without title: long and costly eviction, not appraisable", "Possession status Occupied"));
                break;
            case PossessionStatus.RentedUntil:
                AddOnce(flags, new RedFlag(TenantCode, RedFlagSeverity.High, "Sold with a tenant: the lease continues and you cannot move in", "Possession status RentedUntil"));
                break;
        }

        if (input.Cadastre is { BuiltSquareMetres: { } cadastral } cadastre && cadastral > 0m && input.AdvertisedBuiltSquareMetres is { } advertised && advertised > 0m && cadastre.Match is CadastralMatchQuality.Unit or CadastralMatchQuality.Manual)
        {
            var ratio = Math.Round(advertised / cadastral, 3, MidpointRounding.ToEven);
            var evidence = string.Create(CultureInfo.InvariantCulture, $"Advertised {advertised:0.#} m², Catastro {cadastral:0.#} m² (ratio {ratio:0.00})");
            if (ratio > config.AreaRatioHigh)
            {
                flags.Add(new RedFlag(AreaMismatchHighCode, RedFlagSeverity.High, "Advertised area far above the Catastro: likely unregistered extension or closed terrace", evidence));
            }
            else if (ratio > config.AreaRatioMedium)
            {
                flags.Add(new RedFlag(AreaMismatchCode, RedFlagSeverity.Medium, "Advertised area above the Catastro: ask what is not registered", evidence));
            }
            else if (ratio > config.AreaRatioLow || ratio < config.AreaRatioBelow)
            {
                flags.Add(new RedFlag(AreaMismatchLowCode, RedFlagSeverity.Low, "Advertised area slightly different from the Catastro", evidence));
            }
        }

        if (input.Cadastre is { IsRustic: true })
        {
            AddOnce(flags, new RedFlag(RusticCode, RedFlagSeverity.High, "Rustic land: building limits, legalisations and lower appraisals", "Catastro classifies the parcel as rustic"));
        }

        var year = input.Cadastre?.YearBuilt ?? input.YearBuilt;
        if (year is { } built && input.CurrentYear - built >= config.InspectionAgeYears && !config.InspectionDoneKeywords.Any(keyword => TextKeys.ContainsPhrase(words, keyword)))
        {
            AddOnce(flags, new RedFlag(InspectionCode, RedFlagSeverity.Medium, "Building 50 years or older with no mention of the evaluation report (IEE): ask for it and for pending levies", string.Create(CultureInfo.InvariantCulture, $"Built in {built}")));
        }

        return flags;
    }

    private static void AddOnce(List<RedFlag> flags, RedFlag flag)
    {
        if (flags.All(existing => existing.Code != flag.Code))
        {
            flags.Add(flag);
        }
    }

    public static decimal RiskPenalty(IEnumerable<RedFlag> flags, RedFlagConfig config) =>
        flags.Sum(flag => config.RiskPenalty.TryGetValue(flag.Severity, out var penalty) ? penalty : 0m);
}
