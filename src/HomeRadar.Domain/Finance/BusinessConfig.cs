using System.Text.Json;
using HomeRadar.Domain.Common;
using HomeRadar.Domain.Intelligence;

namespace HomeRadar.Domain.Finance;

public sealed record BusinessConfig
{
    public string DefaultJurisdiction { get; init; } = "ES-IB";

    public required EuriborReference Euribor { get; init; }

    public IReadOnlyList<RateScenario> RateScenarios { get; init; } = [new("base", 0m)];

    public string StressScenario { get; init; } = "base";

    public AffordabilityThresholds Affordability { get; init; } = new();

    public ClosingCostEstimates ClosingCosts { get; init; } = new();

    public RenovationConfig Renovation { get; init; } = new();

    public RedFlagConfig RedFlags { get; init; } = new();

    public NegotiationConfig Negotiation { get; init; } = new();

    public ComparablesConfig Comparables { get; init; } = new();

    public ScoringConfig Scoring { get; init; } = new();

    public BiddingConfig Bidding { get; init; } = new();

    public static BusinessConfig Parse(string json) =>
        JsonSerializer.Deserialize<BusinessConfig>(json, FinanceJson.Options) ?? throw new DomainException("Business configuration payload is empty.");

    public string ToJson() => JsonSerializer.Serialize(this, FinanceJson.Options);
}

public sealed record EuriborReference
{
    public required decimal TwelveMonthPercent { get; init; }

    public required string Month { get; init; }

    public string? SourceUrl { get; init; }
}

public sealed record RateScenario(string Name, decimal EuriborDeltaPercent);

public sealed record AffordabilityThresholds
{
    public decimal BorderlineCashShortfallMax { get; init; } = 5000m;

    public decimal BorderlineDebtToIncomeTolerance { get; init; } = 0.03m;

    public int MaxAgeAtLoanMaturity { get; init; } = 75;

    public decimal RenovationContingencyPercent { get; init; } = 10m;
}

public sealed record ClosingCostEstimates
{
    public decimal GestoriaEur { get; init; } = 400m;

    public decimal AppraisalEur { get; init; } = 450m;

    public string? SourceUrl { get; init; }
}
