using HomeRadar.Domain.Sources;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Domain.Intelligence;

public enum RenovationLevel
{
    ReadyToMoveIn = 0,
    Light = 1,
    Medium = 2,
    Full = 3,
}

public enum EstimateConfidence
{
    Low = 0,
    Medium = 1,
    High = 2,
}

public enum CostUnit
{
    SquareMetre = 0,
    Unit = 1,
    Dwelling = 2,
}

public enum QuantityBasis
{
    Fixed = 0,
    BuiltArea = 1,
    Bathrooms = 2,
    Rooms = 3,
}

public enum RedFlagSeverity
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3,
}

public sealed record CostRange(decimal Min, decimal Probable, decimal Max)
{
    public static CostRange Zero { get; } = new(0m, 0m, 0m);

    public CostRange Times(decimal factor) => new(Min * factor, Probable * factor, Max * factor);

    public CostRange Plus(CostRange other) => new(Min + other.Min, Probable + other.Probable, Max + other.Max);

    public CostRange Rounded() => new(Money.Round(Min), Money.Round(Probable), Money.Round(Max));
}

public sealed record RenovationConfig
{
    public decimal DefaultBuiltAreaSquareMetres { get; init; } = 80m;

    public decimal RegionalFactor { get; init; } = 1m;

    public IReadOnlyList<RenovationLevelCost> Levels { get; init; } = [];

    public IReadOnlyList<RenovationItemCost> Items { get; init; } = [];

    public IReadOnlyList<AgeRule> AgeRules { get; init; } = [];

    public IReadOnlyList<KeywordGroup> Keywords { get; init; } = [];

    public IReadOnlyList<ChecklistGroup> VisitChecklist { get; init; } = [];

    public IReadOnlyList<string> Negations { get; init; } = [];

    public string? RecentRenovationPattern { get; init; }

    public int RecentRenovationMaxAgeYears { get; init; } = 15;

    public decimal PermitAndTaxPercent { get; init; }

    public string? SourceUrl { get; init; }

    public string? Note { get; init; }
}

public sealed record RenovationLevelCost
{
    public required RenovationLevel Level { get; init; }

    public required decimal MinPerSquareMetre { get; init; }

    public required decimal ProbablePerSquareMetre { get; init; }

    public required decimal MaxPerSquareMetre { get; init; }

    public string? Description { get; init; }

    public string? SourceUrl { get; init; }
}

public sealed record RenovationItemCost
{
    public required string Code { get; init; }

    public required string Name { get; init; }

    public required CostUnit Unit { get; init; }

    public QuantityBasis Quantity { get; init; } = QuantityBasis.Fixed;

    public decimal FixedQuantity { get; init; } = 1m;

    public decimal QuantityOffset { get; init; }

    public required decimal Min { get; init; }

    public required decimal Probable { get; init; }

    public required decimal Max { get; init; }

    public IReadOnlyList<RenovationLevel> AppliesTo { get; init; } = [];

    public string? SourceUrl { get; init; }
}

public sealed record AgeRule
{
    public required int BuiltBefore { get; init; }

    public required RenovationLevel MinimumLevel { get; init; }

    public required string Reason { get; init; }
}

public sealed record KeywordGroup
{
    public required RenovationLevel Level { get; init; }

    public IReadOnlyList<string> Phrases { get; init; } = [];
}

public sealed record ChecklistGroup
{
    public required RenovationLevel Level { get; init; }

    public IReadOnlyList<string> Checks { get; init; } = [];
}

public sealed record RedFlagConfig
{
    public IReadOnlyList<RedFlagRule> Rules { get; init; } = [];

    public decimal AreaRatioLow { get; init; } = 1.05m;

    public decimal AreaRatioMedium { get; init; } = 1.15m;

    public decimal AreaRatioHigh { get; init; } = 1.30m;

    public decimal AreaRatioBelow { get; init; } = 0.70m;

    public int InspectionAgeYears { get; init; } = 50;

    public IReadOnlyList<string> InspectionDoneKeywords { get; init; } = [];

    public IReadOnlyDictionary<RedFlagSeverity, decimal> RiskPenalty { get; init; } = new Dictionary<RedFlagSeverity, decimal>
    {
        [RedFlagSeverity.Low] = 10m,
        [RedFlagSeverity.Medium] = 25m,
        [RedFlagSeverity.High] = 50m,
        [RedFlagSeverity.Critical] = 100m,
    };
}

public sealed record RedFlagRule
{
    public required string Code { get; init; }

    public required RedFlagSeverity Severity { get; init; }

    public required string Description { get; init; }

    public IReadOnlyList<string> Keywords { get; init; } = [];

    public IReadOnlyList<string> Exclusions { get; init; } = [];

    public string? SourceUrl { get; init; }
}

public sealed record NegotiationConfig
{
    public IReadOnlyDictionary<SellerType, decimal> BaseMarginPercent { get; init; } = new Dictionary<SellerType, decimal>();

    public decimal? NewBuildBaseMarginPercent { get; init; }

    public decimal PerPriceDropPercent { get; init; }

    public int MaxCountedPriceDrops { get; init; } = 3;

    public decimal LargeCumulativeDropThresholdPercent { get; init; } = 10m;

    public decimal LargeCumulativeDropPercent { get; init; }

    public int DaysOnMarketGraceDays { get; init; } = 60;

    public decimal PerThirtyDaysOnMarketPercent { get; init; }

    public decimal MaxDaysOnMarketPercent { get; init; } = 5m;

    public int FreshListingDays { get; init; } = 14;

    public decimal FreshListingPercent { get; init; }

    public decimal RelistedPercent { get; init; }

    public decimal UrgencyPercent { get; init; }

    public IReadOnlyList<string> UrgencyKeywords { get; init; } = [];

    public decimal NeedsRenovationPercent { get; init; }

    public decimal OverpricedThresholdPercent { get; init; } = 30m;

    public decimal OverpricedPercent { get; init; }

    public IReadOnlyList<int> SeasonalMonths { get; init; } = [];

    public decimal SeasonalPercent { get; init; }

    public decimal MaxMarginPercent { get; init; } = 15m;

    public decimal RangeBelowPercent { get; init; } = 3m;

    public decimal RangeAbovePercent { get; init; } = 4m;

    public string? SourceUrl { get; init; }
}

public sealed record ComparablesConfig
{
    public decimal RadiusMetres { get; init; } = 2_000m;

    public decimal AreaTolerancePercent { get; init; } = 35m;

    public int MinimumCount { get; init; } = 5;

    public int MaximumAgeDays { get; init; } = 365;
}

public sealed record ScoringConfig
{
    public IReadOnlyDictionary<string, decimal> Weights { get; init; } = new Dictionary<string, decimal>(StringComparer.Ordinal);

    public decimal FullScoreDiscountPercent { get; init; } = 25m;

    public decimal FullMarginPercent { get; init; } = 15m;

    public decimal ZeroScoreRenovationSharePercent { get; init; } = 40m;

    public IReadOnlyDictionary<SourceKind, decimal> CompetitionBySourceKind { get; init; } = new Dictionary<SourceKind, decimal>();
}

public sealed record BiddingConfig
{
    public decimal OverpayTolerancePercent { get; init; } = 5m;
}
