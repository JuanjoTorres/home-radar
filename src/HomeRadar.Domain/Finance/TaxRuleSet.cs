using System.Text.Json;
using System.Text.Json.Serialization;
using HomeRadar.Domain.Common;

namespace HomeRadar.Domain.Finance;

public sealed class TaxRuleSet : Entity
{
    private TaxRuleDocument? _document;

    private TaxRuleSet()
    {
    }

    public TaxRuleSet(TaxRuleDocument document, DateOnly validFrom, int revision, DateOnly? verifiedOn, string sourceFile, DateTimeOffset loadedAt)
    {
        Jurisdiction = document.Jurisdiction;
        ValidFrom = validFrom;
        Revision = revision;
        VerifiedOn = verifiedOn;
        SourceFile = sourceFile;
        LoadedAt = loadedAt;
        DocumentJson = JsonSerializer.Serialize(document, FinanceJson.Options);
        _document = document;
    }

    public string Jurisdiction { get; private init; } = string.Empty;

    public DateOnly ValidFrom { get; private init; }

    public int Revision { get; private init; }

    public DateOnly? VerifiedOn { get; private init; }

    public string SourceFile { get; private init; } = string.Empty;

    public DateTimeOffset LoadedAt { get; private init; }

    public string DocumentJson { get; private init; } = "{}";

    public TaxRuleDocument Document => _document ??= JsonSerializer.Deserialize<TaxRuleDocument>(DocumentJson, FinanceJson.Options)!;
}

public static class Jurisdictions
{
    public const string Spain = "ES";

    public static IReadOnlyList<string> Chain(string jurisdiction)
    {
        var parts = jurisdiction.Split('-', StringSplitOptions.RemoveEmptyEntries);
        return Enumerable.Range(1, parts.Length).Select(length => string.Join('-', parts.Take(length))).ToList();
    }
}

public static class FinanceJson
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };
}

public sealed record TaxRuleDocument
{
    public required string Jurisdiction { get; init; }

    public string? Name { get; init; }

    public IReadOnlyList<Subdivision> Subdivisions { get; init; } = [];

    public IReadOnlyList<TaxRule> Rules { get; init; } = [];

    public IReadOnlyList<FeeTariff> Fees { get; init; } = [];

    public IReadOnlyList<AidProgram> AidPrograms { get; init; } = [];
}

public sealed record Subdivision
{
    public required string Code { get; init; }

    public required string Name { get; init; }

    public bool IsDefault { get; init; }

    public IReadOnlyList<string> Municipalities { get; init; } = [];

    public IReadOnlyList<ValueLimit> ValueLimits { get; init; } = [];

    public ValueLimit? ValueLimitOn(DateOnly date) =>
        ValueLimits.Where(limit => limit.IsValidOn(date)).OrderByDescending(limit => limit.ValidFrom).FirstOrDefault();
}

public sealed record ValueLimit
{
    public required DateOnly ValidFrom { get; init; }

    public DateOnly? ValidTo { get; init; }

    public required decimal Amount { get; init; }

    public string? SourceUrl { get; init; }

    public bool IsValidOn(DateOnly date) => ValidFrom <= date && (ValidTo is null || date <= ValidTo);
}

public enum TaxKind
{
    TransferTax = 0,
    ValueAddedTax = 1,
    StampDuty = 2,
}

public enum PurchaseKind
{
    Resale = 0,
    NewBuild = 1,
}

public enum TaxRuleKind
{
    ProgressiveScale = 0,
    ReducedRate = 1,
    FlatRate = 2,
    QuotaBonus = 3,
}

public enum TaxRuleRole
{
    Standard = 0,
    Override = 1,
    Relief = 2,
}

public sealed record TaxRule
{
    public required string Id { get; init; }

    public required TaxKind Tax { get; init; }

    public required PurchaseKind AppliesTo { get; init; }

    public required TaxRuleKind Kind { get; init; }

    public TaxRuleRole Role { get; init; } = TaxRuleRole.Standard;

    public EligibilityConditions Conditions { get; init; } = new();

    public IReadOnlyList<RateBracket> Brackets { get; init; } = [];

    public decimal? Rate { get; init; }

    public decimal? RateAppliesUpTo { get; init; }

    public decimal? ExcessRate { get; init; }

    public decimal? Bonus { get; init; }

    public decimal? BonusAppliesUpTo { get; init; }

    public DateOnly? ValidFrom { get; init; }

    public DateOnly? ValidTo { get; init; }

    public string? Description { get; init; }

    public string? SourceUrl { get; init; }

    public bool Verified { get; init; } = true;

    public string? Note { get; init; }

    public bool IsValidOn(DateOnly date) => (ValidFrom is null || ValidFrom <= date) && (ValidTo is null || date <= ValidTo);

    public bool IsBonus => Kind == TaxRuleKind.QuotaBonus;
}

public sealed record RateBracket(decimal? UpTo, decimal Rate);

public enum FeeKind
{
    NotaryDeed = 0,
    LandRegistry = 1,
}

public sealed record FeeTariff
{
    public required string Id { get; init; }

    public required FeeKind Kind { get; init; }

    public IReadOnlyList<TariffBracket> Brackets { get; init; } = [];

    public decimal Reduction { get; init; }

    public decimal Vat { get; init; }

    public decimal ExtrasEur { get; init; }

    public decimal? MinEur { get; init; }

    public decimal? MaxEur { get; init; }

    public string? Description { get; init; }

    public string? SourceUrl { get; init; }

    public bool Verified { get; init; } = true;

    public string? Note { get; init; }
}

public sealed record TariffBracket(decimal? UpTo, decimal? Fixed, decimal? Permille);

public enum AidKind
{
    LoanGuarantee = 0,
    DirectGrant = 1,
}

public sealed record AidProgram
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required AidKind Kind { get; init; }

    public EligibilityConditions Conditions { get; init; } = new();

    public IReadOnlyList<GuaranteeTier> GuaranteeTiers { get; init; } = [];

    public decimal GuaranteeFeePercent { get; init; }

    public decimal? GrantAmount { get; init; }

    public bool BudgetLimited { get; init; }

    public DateOnly? ValidFrom { get; init; }

    public DateOnly? ValidTo { get; init; }

    public string? SourceUrl { get; init; }

    public bool Verified { get; init; } = true;

    public string? Note { get; init; }

    public bool IsValidOn(DateOnly date) => (ValidFrom is null || ValidFrom <= date) && (ValidTo is null || date <= ValidTo);
}

public sealed record GuaranteeTier
{
    public EligibilityConditions Conditions { get; init; } = new();

    public required decimal MaxLoanToValue { get; init; }
}
