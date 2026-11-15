using System.Text.Json;
using HomeRadar.Domain.Configuration;
using HomeRadar.Domain.Finance;

namespace HomeRadar.Api.Contracts;

/// <summary>Version of the business assumptions that are not law: Euríbor reference, stress scenarios, verdict thresholds and closing-cost estimates.</summary>
public sealed record BusinessConfigDto
{
    /// <summary>Version id; analyses store it.</summary>
    public required Guid Id { get; init; }

    /// <summary>First day this version applies.</summary>
    /// <example>2026-11-01</example>
    public required DateOnly ValidFrom { get; init; }

    /// <summary>Revision within the same validity date; a higher revision replaces a lower one.</summary>
    public required int Revision { get; init; }

    /// <summary>Where the figures come from.</summary>
    public required string Source { get; init; }

    /// <summary>When the version was loaded from the seed files (UTC).</summary>
    public required DateTimeOffset LoadedAt { get; init; }

    /// <summary>Jurisdiction whose tax rules apply, e.g. ES-IB.</summary>
    public required string DefaultJurisdiction { get; init; }

    /// <summary>Euríbor 12M reference.</summary>
    public required EuriborDto Euribor { get; init; }

    /// <summary>Scenarios used for variable and mixed loans: Euríbor reference plus a delta in percentage points.</summary>
    public required IReadOnlyList<RateScenarioDto> RateScenarios { get; init; }

    /// <summary>Scenario used as stress test for the verdict.</summary>
    /// <example>plus2</example>
    public required string StressScenario { get; init; }

    /// <summary>Verdict thresholds.</summary>
    public required AffordabilityThresholdsDto Affordability { get; init; }

    /// <summary>Market estimates of closing costs without an official tariff.</summary>
    public required ClosingCostEstimatesDto ClosingCosts { get; init; }

    public static BusinessConfigDto From(BusinessConfigVersion version)
    {
        var config = version.Config;
        return new BusinessConfigDto
        {
            Id = version.Id,
            ValidFrom = version.ValidFrom,
            Revision = version.Revision,
            Source = version.SourceDescription,
            LoadedAt = version.LoadedAt,
            DefaultJurisdiction = config.DefaultJurisdiction,
            Euribor = new EuriborDto(config.Euribor.TwelveMonthPercent, config.Euribor.Month, config.Euribor.SourceUrl),
            RateScenarios = config.RateScenarios.Select(scenario => new RateScenarioDto(scenario.Name, scenario.EuriborDeltaPercent)).ToList(),
            StressScenario = config.StressScenario,
            Affordability = new AffordabilityThresholdsDto(config.Affordability.BorderlineCashShortfallMax, config.Affordability.BorderlineDebtToIncomeTolerance, config.Affordability.MaxAgeAtLoanMaturity, config.Affordability.RenovationContingencyPercent),
            ClosingCosts = new ClosingCostEstimatesDto(config.ClosingCosts.GestoriaEur, config.ClosingCosts.AppraisalEur, config.ClosingCosts.SourceUrl),
        };
    }
}

/// <summary>Euríbor 12M reference.</summary>
/// <param name="TwelveMonthPercent">Monthly average in %, e.g. 3.239.</param>
/// <param name="Month">Month of the average, YYYY-MM.</param>
/// <param name="SourceUrl">Banco de España source.</param>
public sealed record EuriborDto(decimal TwelveMonthPercent, string Month, string? SourceUrl);

/// <summary>Euríbor scenario.</summary>
/// <param name="Name">Scenario name.</param>
/// <param name="EuriborDeltaPercent">Percentage points added to the reference.</param>
public sealed record RateScenarioDto(string Name, decimal EuriborDeltaPercent);

/// <summary>Verdict thresholds.</summary>
/// <param name="BorderlineCashShortfallMax">Largest savings shortfall in EUR still considered Borderline.</param>
/// <param name="BorderlineDebtToIncomeTolerance">Debt-to-income points above the profile limit still considered Borderline (fraction).</param>
/// <param name="MaxAgeAtLoanMaturity">Usual maximum age at the end of the loan.</param>
/// <param name="RenovationContingencyPercent">Contingency added to renovation budgets, in %.</param>
public sealed record AffordabilityThresholdsDto(decimal BorderlineCashShortfallMax, decimal BorderlineDebtToIncomeTolerance, int MaxAgeAtLoanMaturity, decimal RenovationContingencyPercent);

/// <summary>Closing-cost estimates.</summary>
/// <param name="GestoriaEur">Gestoria for the purchase deed, EUR.</param>
/// <param name="AppraisalEur">Official appraisal paid by the borrower, EUR.</param>
/// <param name="SourceUrl">Where the estimate comes from.</param>
public sealed record ClosingCostEstimatesDto(decimal GestoriaEur, decimal AppraisalEur, string? SourceUrl);

/// <summary>Immutable set of tax rules, island limits, fee tariffs and aid programmes for one jurisdiction.</summary>
public sealed record TaxRuleSetDto
{
    /// <summary>Tax rule set id.</summary>
    public required Guid Id { get; init; }

    /// <summary>ISO 3166 code: ES or ES-IB.</summary>
    public required string Jurisdiction { get; init; }

    /// <summary>Display name.</summary>
    public string? Name { get; init; }

    /// <summary>First day the set applies.</summary>
    public required DateOnly ValidFrom { get; init; }

    /// <summary>Revision within that date.</summary>
    public required int Revision { get; init; }

    /// <summary>Day the figures were last checked against official sources.</summary>
    public DateOnly? VerifiedOn { get; init; }

    /// <summary>Seed file it was loaded from.</summary>
    public required string SourceFile { get; init; }

    /// <summary>Complete rule document as stored: <c>subdivisions</c> (islands with value limits), <c>rules</c> (rate, scale and bonus rules with their eligibility conditions), <c>fees</c> (notary and registry tariffs) and <c>aidPrograms</c>. Rates and bonuses are fractions; amounts are EUR.</summary>
    public required JsonElement Document { get; init; }

    public static TaxRuleSetDto From(TaxRuleSet set)
    {
        using var document = JsonDocument.Parse(set.DocumentJson);
        return new TaxRuleSetDto
        {
            Id = set.Id,
            Jurisdiction = set.Jurisdiction,
            Name = set.Document.Name,
            ValidFrom = set.ValidFrom,
            Revision = set.Revision,
            VerifiedOn = set.VerifiedOn,
            SourceFile = set.SourceFile,
            Document = document.RootElement.Clone(),
        };
    }
}
