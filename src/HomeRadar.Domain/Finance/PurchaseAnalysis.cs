using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Domain.Finance;

public enum AffordabilityVerdict
{
    Affordable = 0,
    Borderline = 1,
    NotAffordable = 2,
}

public enum VerdictReasonCode
{
    CashShortfall = 0,
    DebtToIncomeAboveLimit = 1,
    StressScenarioAboveLimit = 2,
    LoanEndsAfterMaximumAge = 3,
}

public sealed record VerdictReason(VerdictReasonCode Code, bool Blocking, string Message, Money? Amount, decimal? Ratio);

public sealed record PurchaseScenario(
    Money Price,
    PurchaseKind Purchase,
    PropertyType PropertyType,
    HousingRegime HousingRegime,
    string? Municipality,
    Money? CadastralReferenceValue,
    Money RenovationCost,
    bool RenovationEstimated,
    DateOnly On)
{
    public PurchaseScenario WithPrice(Money price) => this with { Price = price };
}

public sealed record FinancingInputs(
    Money Savings,
    Money NetMonthlyIncome,
    Money ExistingMonthlyDebt,
    decimal MaxDebtToIncomeRatio,
    MortgageTerms Mortgage,
    BuyerProfile Buyer)
{
    public static FinancingInputs From(FinancingProfile profile) =>
        new(profile.Savings, profile.NetMonthlyIncome, profile.ExistingMonthlyDebt, profile.MaxDebtToIncomeRatio, profile.Mortgage, profile.Buyer);
}

public sealed record LocationResolution(string Jurisdiction, string SubdivisionCode, string SubdivisionName, bool Assumed, decimal? ValueLimit);

public sealed record LoanOption(decimal LoanToValue, string? GuaranteeProgramId, decimal GuaranteeFeePercent);

public sealed record PurchaseAnalysis(
    LocationResolution Location,
    PurchaseKind Purchase,
    Money Price,
    Money TaxableValue,
    TaxAssessment Taxes,
    IReadOnlyList<CostLine> Costs,
    Money TotalAcquisitionCost,
    LoanOption LoanOption,
    Money Loan,
    Money CashNeeded,
    Money Savings,
    Money RemainingSavings,
    IReadOnlyList<PaymentSchedule> Payments,
    string StressScenario,
    decimal DebtToIncome,
    decimal StressDebtToIncome,
    int AgeAtLoanMaturity,
    AffordabilityVerdict Verdict,
    IReadOnlyList<VerdictReason> Reasons,
    IReadOnlyList<string> Warnings);

public sealed record AidAssessment(
    string ProgramId,
    string Name,
    AidKind Kind,
    EligibilityOutcome Outcome,
    IReadOnlyList<ConditionCheck> Checks,
    decimal? MaxLoanToValue,
    Money? GrantAmount,
    bool BudgetLimited,
    bool Verified,
    string? SourceUrl,
    string? Note);

public sealed record MaximumPrice(Money? Price, AffordabilityVerdict Target, IReadOnlyList<VerdictReason> BindingConstraints);

public sealed record FinancialAnalysis(
    PurchaseAnalysis Base,
    IReadOnlyList<AidAssessment> Aid,
    PurchaseAnalysis? WithGuarantee,
    MaximumPrice? MaximumPrice,
    MaximumPrice? MaximumPriceWithGuarantee);
