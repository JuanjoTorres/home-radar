using HomeRadar.Application.Analysis;
using HomeRadar.Domain.Finance;

namespace HomeRadar.Api.Contracts;

/// <summary>Configuration versions an analysis was computed with; together with the stored inputs they make it reproducible.</summary>
public sealed record AnalysisConfigurationDto
{
    /// <summary>Business configuration version id (Euríbor, thresholds, closing-cost estimates).</summary>
    public required Guid BusinessConfigVersionId { get; init; }

    /// <summary>Date from which that business configuration applies.</summary>
    /// <example>2026-11-01</example>
    public required DateOnly BusinessConfigValidFrom { get; init; }

    /// <summary>Revision of that business configuration.</summary>
    public required int BusinessConfigRevision { get; init; }

    /// <summary>Tax rule sets applied, from the state to the most specific jurisdiction.</summary>
    public required IReadOnlyList<TaxRuleSetReferenceDto> TaxRuleSets { get; init; }

    /// <summary>Calculation date: rules, island limits and aid programmes are those valid on this day.</summary>
    /// <example>2026-11-15</example>
    public required DateOnly On { get; init; }

    public static AnalysisConfigurationDto From(FinanceConfiguration configuration) => new()
    {
        BusinessConfigVersionId = configuration.Version.Id,
        BusinessConfigValidFrom = configuration.Version.ValidFrom,
        BusinessConfigRevision = configuration.Version.Revision,
        TaxRuleSets = configuration.TaxRuleSets.Select(set => new TaxRuleSetReferenceDto(set.Id, set.Jurisdiction, set.ValidFrom, set.Revision)).ToList(),
        On = configuration.On,
    };

    public static AnalysisConfigurationDto From(ListingAnalysisInput input, DateOnly businessConfigValidFrom) => new()
    {
        BusinessConfigVersionId = input.BusinessConfigVersionId,
        BusinessConfigValidFrom = businessConfigValidFrom,
        BusinessConfigRevision = input.BusinessConfigRevision,
        TaxRuleSets = input.TaxRuleSets.Select(set => new TaxRuleSetReferenceDto(set.Id, set.Jurisdiction, set.ValidFrom, set.Revision)).ToList(),
        On = input.On,
    };
}

/// <summary>Reference to an immutable tax rule set.</summary>
/// <param name="Id">Tax rule set id.</param>
/// <param name="Jurisdiction">ISO 3166 code: ES (state) or ES-IB (Balearic Islands).</param>
/// <param name="ValidFrom">First day the set applies.</param>
/// <param name="Revision">Revision within that validity date.</param>
public sealed record TaxRuleSetReferenceDto(Guid Id, string Jurisdiction, DateOnly ValidFrom, int Revision);

/// <summary>Stored analysis of one listing with one financing profile.</summary>
public sealed record ListingAnalysisDto
{
    /// <summary>Analysis id; the same inputs return the same stored analysis.</summary>
    public required Guid AnalysisId { get; init; }

    /// <summary>Listing analysed.</summary>
    public required Guid ListingId { get; init; }

    /// <summary>Financing profile used.</summary>
    public required Guid FinancingProfileId { get; init; }

    /// <summary>When the analysis was first computed (UTC).</summary>
    public required DateTimeOffset CalculatedAt { get; init; }

    /// <summary>True when an identical earlier analysis (same listing content, profile version and configuration) was returned.</summary>
    public required bool Reused { get; init; }

    /// <summary>Configuration versions used.</summary>
    public required AnalysisConfigurationDto Configuration { get; init; }

    /// <summary>Result of the analysis.</summary>
    public required FinancialAnalysisDto Result { get; init; }
}

/// <summary>Result of a free simulation (no listing involved).</summary>
public sealed record SimulationResultDto
{
    /// <summary>Configuration versions used.</summary>
    public required AnalysisConfigurationDto Configuration { get; init; }

    /// <summary>Euríbor 12M in % used for variable and mixed payments, after any override.</summary>
    /// <example>3.239</example>
    public required decimal EuriborPercent { get; init; }

    /// <summary>Result of the analysis.</summary>
    public required FinancialAnalysisDto Result { get; init; }
}

/// <summary>Complete financial analysis of a purchase.</summary>
public sealed record FinancialAnalysisDto
{
    /// <summary>Analysis with the profile's own loan-to-value.</summary>
    public required PurchaseAnalysisDto Base { get; init; }

    /// <summary>Same purchase financed with the best guarantee programme the buyer qualifies for (ICO or IBAVI), when it raises the loan-to-value; otherwise null.</summary>
    public PurchaseAnalysisDto? WithGuarantee { get; init; }

    /// <summary>Eligibility for every aid programme valid on the calculation date.</summary>
    public required IReadOnlyList<AidAssessmentDto> Aid { get; init; }

    /// <summary>Highest price that keeps the verdict Affordable with the profile's loan-to-value, all other assumptions equal.</summary>
    public MaximumPriceDto? MaximumPrice { get; init; }

    /// <summary>Same with an eligible guarantee, when it is higher than <c>maximumPrice</c>.</summary>
    public MaximumPriceDto? MaximumPriceWithGuarantee { get; init; }

    public static FinancialAnalysisDto From(FinancialAnalysis analysis) => new()
    {
        Base = PurchaseAnalysisDto.From(analysis.Base),
        WithGuarantee = analysis.WithGuarantee is { } guaranteed ? PurchaseAnalysisDto.From(guaranteed) : null,
        Aid = analysis.Aid.Select(AidAssessmentDto.From).ToList(),
        MaximumPrice = analysis.MaximumPrice is { } maximum ? MaximumPriceDto.From(maximum) : null,
        MaximumPriceWithGuarantee = analysis.MaximumPriceWithGuarantee is { } guaranteedMaximum ? MaximumPriceDto.From(guaranteedMaximum) : null,
    };
}

/// <summary>Cost, financing and verdict of one way of financing a purchase.</summary>
public sealed record PurchaseAnalysisDto
{
    /// <summary>Affordable, Borderline or NotAffordable. Affordable: savings cover deposit, taxes and costs, and the payment stays within the debt-to-income limit (also under the stress scenario for variable and mixed loans). Borderline: a small gap (cash shortfall up to the configured tolerance, debt-to-income up to the limit plus the tolerance, the stress scenario over the limit, or a loan ending after the usual maximum age). NotAffordable: any larger gap.</summary>
    /// <example>Affordable</example>
    public required string Verdict { get; init; }

    /// <summary>Why the verdict is not Affordable; empty when it is.</summary>
    public required IReadOnlyList<VerdictReasonDto> Reasons { get; init; }

    /// <summary>Resale (transfer tax) or NewBuild (VAT and stamp duty).</summary>
    public required string PurchaseKind { get; init; }

    /// <summary>Price analysed.</summary>
    public required MoneyDto Price { get; init; }

    /// <summary>Tax base for transfer tax and stamp duty: the higher of price and cadastral reference value (art. 10 TRLITPAJD).</summary>
    public required MoneyDto TaxableValue { get; init; }

    /// <summary>Jurisdiction and island used for the tax rules.</summary>
    public required LocationDto Location { get; init; }

    /// <summary>Taxes due and the reliefs considered.</summary>
    public required TaxAssessmentDto Taxes { get; init; }

    /// <summary>Closing costs, guarantee fees and renovation, excluding taxes.</summary>
    public required IReadOnlyList<CostLineDto> Costs { get; init; }

    /// <summary>Price + taxes + every cost line.</summary>
    public required MoneyDto TotalAcquisitionCost { get; init; }

    /// <summary>Mortgage amount: price × loan-to-value (appraisal assumed equal to price).</summary>
    public required MoneyDto Loan { get; init; }

    /// <summary>Loan-to-value applied, as a fraction (0–1).</summary>
    /// <example>0.8</example>
    public required decimal LoanToValue { get; init; }

    /// <summary>Guarantee programme behind the loan-to-value, when any.</summary>
    public string? GuaranteeProgramId { get; init; }

    /// <summary>Cash needed on signing day: total acquisition cost − loan.</summary>
    public required MoneyDto CashNeeded { get; init; }

    /// <summary>Savings of the profile.</summary>
    public required MoneyDto Savings { get; init; }

    /// <summary>Savings left after signing; negative means a shortfall.</summary>
    public required MoneyDto RemainingSavings { get; init; }

    /// <summary>Payment schedules: one for fixed loans; one per Euríbor scenario for variable and mixed loans. Monthly payment = L·i / (1 − (1 + i)^−n) with i = annual rate / 12 and n months; after the initial period the outstanding balance is re-amortised at Euríbor + spread.</summary>
    public required IReadOnlyList<PaymentScheduleDto> Payments { get; init; }

    /// <summary>(Highest monthly payment in the base scenario + existing debts) / net monthly income.</summary>
    /// <example>0.2775</example>
    public required decimal DebtToIncome { get; init; }

    /// <summary>Euríbor scenario used as stress test.</summary>
    /// <example>plus2</example>
    public required string StressScenario { get; init; }

    /// <summary>Debt-to-income under the stress scenario.</summary>
    public required decimal StressDebtToIncome { get; init; }

    /// <summary>Buyer's age when the loan ends.</summary>
    public required int AgeAtLoanMaturity { get; init; }

    /// <summary>Assumptions and data gaps to keep in mind (unknown cadastral value, interpretations pending confirmation…).</summary>
    public required IReadOnlyList<string> Warnings { get; init; }

    public static PurchaseAnalysisDto From(PurchaseAnalysis analysis) => new()
    {
        Verdict = analysis.Verdict.ToString(),
        Reasons = analysis.Reasons.Select(VerdictReasonDto.From).ToList(),
        PurchaseKind = analysis.Purchase.ToString(),
        Price = MoneyDto.From(analysis.Price),
        TaxableValue = MoneyDto.From(analysis.TaxableValue),
        Location = LocationDto.From(analysis.Location),
        Taxes = TaxAssessmentDto.From(analysis.Taxes),
        Costs = analysis.Costs.Select(CostLineDto.From).ToList(),
        TotalAcquisitionCost = MoneyDto.From(analysis.TotalAcquisitionCost),
        Loan = MoneyDto.From(analysis.Loan),
        LoanToValue = analysis.LoanOption.LoanToValue,
        GuaranteeProgramId = analysis.LoanOption.GuaranteeProgramId,
        CashNeeded = MoneyDto.From(analysis.CashNeeded),
        Savings = MoneyDto.From(analysis.Savings),
        RemainingSavings = MoneyDto.From(analysis.RemainingSavings),
        Payments = analysis.Payments.Select(PaymentScheduleDto.From).ToList(),
        DebtToIncome = analysis.DebtToIncome,
        StressScenario = analysis.StressScenario,
        StressDebtToIncome = analysis.StressDebtToIncome,
        AgeAtLoanMaturity = analysis.AgeAtLoanMaturity,
        Warnings = analysis.Warnings,
    };
}

/// <summary>Reason that keeps a purchase from being Affordable.</summary>
/// <param name="Code">CashShortfall, DebtToIncomeAboveLimit, StressScenarioAboveLimit or LoanEndsAfterMaximumAge.</param>
/// <param name="Blocking">True when it alone makes the purchase NotAffordable.</param>
/// <param name="Message">Explanation in English.</param>
/// <param name="Amount">Money involved, e.g. the savings shortfall.</param>
/// <param name="Ratio">Ratio involved, e.g. the debt-to-income (fraction).</param>
public sealed record VerdictReasonDto(string Code, bool Blocking, string Message, MoneyDto? Amount, decimal? Ratio)
{
    public static VerdictReasonDto From(VerdictReason reason) => new(reason.Code.ToString(), reason.Blocking, reason.Message, MoneyDto.From(reason.Amount), reason.Ratio);
}

/// <summary>Where the tax rules were resolved.</summary>
/// <param name="Jurisdiction">ISO 3166-2 code, e.g. ES-IB.</param>
/// <param name="SubdivisionCode">Island code: MALLORCA, MENORCA, EIVISSA or FORMENTERA.</param>
/// <param name="SubdivisionName">Island name.</param>
/// <param name="Assumed">True when the municipality was unknown or not recognised and the default island was assumed.</param>
/// <param name="ValueLimit">Island value limit in EUR for the reduced rates on the calculation date (DA 5.ª TR).</param>
public sealed record LocationDto(string Jurisdiction, string SubdivisionCode, string SubdivisionName, bool Assumed, decimal? ValueLimit)
{
    public static LocationDto From(LocationResolution location) => new(location.Jurisdiction, location.SubdivisionCode, location.SubdivisionName, location.Assumed, location.ValueLimit);
}

/// <summary>Taxes due on the purchase.</summary>
public sealed record TaxAssessmentDto
{
    /// <summary>Sum of every tax line.</summary>
    public required MoneyDto Total { get; init; }

    /// <summary>One line per tax: TransferTax (ITP) for resale; ValueAddedTax (IVA) and StampDuty (AJD) for new homes.</summary>
    public required IReadOnlyList<TaxLineDto> Lines { get; init; }

    /// <summary>Every reduced rate and bonus considered, with the outcome of each requirement.</summary>
    public required IReadOnlyList<ReliefAssessmentDto> Reliefs { get; init; }

    public static TaxAssessmentDto From(TaxAssessment taxes) => new()
    {
        Total = MoneyDto.From(taxes.Total),
        Lines = taxes.Lines.Select(TaxLineDto.From).ToList(),
        Reliefs = taxes.Reliefs.Select(ReliefAssessmentDto.From).ToList(),
    };
}

/// <summary>Tax computed with the rule that applied.</summary>
/// <param name="Tax">TransferTax, ValueAddedTax or StampDuty.</param>
/// <param name="RuleId">Rule applied, e.g. itp_under36_first_home_2.</param>
/// <param name="TaxBase">Tax base.</param>
/// <param name="Quota">Tax due after bonuses.</param>
/// <param name="EffectiveRate">Quota / base as a fraction.</param>
/// <param name="Explanation">Rule (and bonus) description with its legal reference.</param>
/// <param name="BonusRuleId">Bonus applied on top, if any.</param>
/// <param name="BonusAmount">Amount removed by the bonus.</param>
/// <param name="SourceUrl">Official source of the rule.</param>
public sealed record TaxLineDto(string Tax, string RuleId, MoneyDto TaxBase, MoneyDto Quota, decimal EffectiveRate, string Explanation, string? BonusRuleId, MoneyDto BonusAmount, string? SourceUrl)
{
    public static TaxLineDto From(TaxLine line) => new(line.Tax.ToString(), line.RuleId, MoneyDto.From(line.TaxBase), MoneyDto.From(line.Quota), line.EffectiveRate, line.Explanation, line.BonusRuleId, MoneyDto.From(line.BonusAmount), line.SourceUrl);
}

/// <summary>Reduced rate or bonus that was evaluated.</summary>
/// <param name="RuleId">Rule id.</param>
/// <param name="Tax">Tax it reduces.</param>
/// <param name="Outcome">Met, NotMet or Unknown (some requirement cannot be checked with the profile data).</param>
/// <param name="Applied">Whether it was the option finally applied.</param>
/// <param name="Checks">Outcome of every requirement.</param>
/// <param name="PotentialSaving">For Unknown outcomes: how much less tax would be paid if the missing requirements were met.</param>
/// <param name="Description">What the rule grants.</param>
public sealed record ReliefAssessmentDto(string RuleId, string Tax, string Outcome, bool Applied, IReadOnlyList<ConditionCheckDto> Checks, MoneyDto? PotentialSaving, string? Description)
{
    public static ReliefAssessmentDto From(ReliefAssessment relief) => new(relief.RuleId, relief.Tax.ToString(), relief.Outcome.ToString(), relief.Applied, relief.Checks.Select(ConditionCheckDto.From).ToList(), MoneyDto.From(relief.PotentialSaving), relief.Description);
}

/// <summary>Outcome of one requirement.</summary>
/// <param name="Condition">Requirement, e.g. ageMax or taxableIncomeMax.</param>
/// <param name="Outcome">Met, NotMet or Unknown.</param>
/// <param name="Detail">Values compared.</param>
public sealed record ConditionCheckDto(string Condition, string Outcome, string Detail)
{
    public static ConditionCheckDto From(ConditionCheck check) => new(check.Condition, check.Outcome.ToString(), check.Detail);
}

/// <summary>Cost other than taxes.</summary>
/// <param name="Category">Notary, LandRegistry, Gestoria, Appraisal, MortgageOpeningFee, GuaranteeFee, Renovation or RenovationContingency.</param>
/// <param name="Code">Tariff or programme id.</param>
/// <param name="Amount">Amount including VAT.</param>
/// <param name="Explanation">How it is computed.</param>
/// <param name="Estimated">True for market estimates rather than official tariffs.</param>
/// <param name="SourceUrl">Source of the tariff or estimate.</param>
public sealed record CostLineDto(string Category, string Code, MoneyDto Amount, string Explanation, bool Estimated, string? SourceUrl)
{
    public static CostLineDto From(CostLine line) => new(line.Category.ToString(), line.Code, MoneyDto.From(line.Amount), line.Explanation, line.Estimated, line.SourceUrl);
}

/// <summary>Monthly payments under one Euríbor scenario.</summary>
/// <param name="Scenario">Scenario name, e.g. base, plus1, plus2.</param>
/// <param name="EuriborPercent">Euríbor 12M in % in this scenario.</param>
/// <param name="Periods">Periods with a constant payment.</param>
/// <param name="MaxMonthlyPayment">Highest monthly payment of the schedule.</param>
/// <param name="TotalInterest">Interest paid over the whole term.</param>
public sealed record PaymentScheduleDto(string Scenario, decimal EuriborPercent, IReadOnlyList<PaymentPeriodDto> Periods, MoneyDto MaxMonthlyPayment, MoneyDto TotalInterest)
{
    public static PaymentScheduleDto From(PaymentSchedule schedule) => new(schedule.Scenario, schedule.EuriborPercent, schedule.Periods.Select(period => new PaymentPeriodDto(period.FromMonth, period.ToMonth, period.AnnualRatePercent, MoneyDto.From(period.MonthlyPayment))).ToList(), MoneyDto.From(schedule.MaxMonthlyPayment), MoneyDto.From(schedule.TotalInterest));
}

/// <summary>Months with the same payment.</summary>
/// <param name="FromMonth">First month (1-based).</param>
/// <param name="ToMonth">Last month, inclusive.</param>
/// <param name="AnnualRatePercent">Nominal annual rate in %.</param>
/// <param name="MonthlyPayment">Monthly payment.</param>
public sealed record PaymentPeriodDto(int FromMonth, int ToMonth, decimal AnnualRatePercent, MoneyDto MonthlyPayment);

/// <summary>Eligibility for an aid programme.</summary>
/// <param name="ProgramId">Programme id, e.g. ibavi_nova_hipoteca_jove.</param>
/// <param name="Name">Programme name.</param>
/// <param name="Kind">LoanGuarantee or DirectGrant.</param>
/// <param name="Outcome">Met, NotMet or Unknown.</param>
/// <param name="Checks">Outcome of every requirement.</param>
/// <param name="MaxLoanToValue">For guarantees: loan-to-value it allows for this buyer.</param>
/// <param name="GrantAmount">For grants: amount paid after the purchase (never counted as cash on signing).</param>
/// <param name="BudgetLimited">True when the programme can run out of budget.</param>
/// <param name="Verified">False when some figure is pending official confirmation.</param>
/// <param name="SourceUrl">Official source.</param>
/// <param name="Note">Caveats.</param>
public sealed record AidAssessmentDto(string ProgramId, string Name, string Kind, string Outcome, IReadOnlyList<ConditionCheckDto> Checks, decimal? MaxLoanToValue, MoneyDto? GrantAmount, bool BudgetLimited, bool Verified, string? SourceUrl, string? Note)
{
    public static AidAssessmentDto From(AidAssessment aid) => new(aid.ProgramId, aid.Name, aid.Kind.ToString(), aid.Outcome.ToString(), aid.Checks.Select(ConditionCheckDto.From).ToList(), aid.MaxLoanToValue, MoneyDto.From(aid.GrantAmount), aid.BudgetLimited, aid.Verified, aid.SourceUrl, aid.Note);
}

/// <summary>Highest price that keeps the target verdict, found by bisection to the cent.</summary>
/// <param name="Price">Maximum price; null when no price from 1,000 EUR reaches the target.</param>
/// <param name="Target">Verdict required: Affordable or Borderline.</param>
/// <param name="BindingConstraints">Reasons that appear one cent above the maximum: what limits the price.</param>
public sealed record MaximumPriceDto(MoneyDto? Price, string Target, IReadOnlyList<VerdictReasonDto> BindingConstraints)
{
    public static MaximumPriceDto From(MaximumPrice maximum) => new(MoneyDto.From(maximum.Price), maximum.Target.ToString(), maximum.BindingConstraints.Select(VerdictReasonDto.From).ToList());
}

/// <summary>Financing assumptions given inline instead of a stored profile.</summary>
public sealed record FinancingAssumptionsDto
{
    /// <summary>Savings available for deposit and costs.</summary>
    public required MoneyDto Savings { get; init; }

    /// <summary>Net monthly income of all borrowers.</summary>
    public required MoneyDto NetMonthlyIncome { get; init; }

    /// <summary>Existing monthly debt payments.</summary>
    public MoneyDto ExistingMonthlyDebt { get; init; } = new(0);

    /// <summary>Maximum debt-to-income ratio (0.1–0.6).</summary>
    public decimal MaxDebtToIncomeRatio { get; init; } = 0.35m;

    /// <summary>Mortgage assumptions.</summary>
    public required MortgageTermsDto Mortgage { get; init; }

    /// <summary>Buyer circumstances.</summary>
    public required BuyerProfileDto Buyer { get; init; }
}

/// <summary>Free simulation of a purchase.</summary>
public sealed record SimulationRequestDto
{
    /// <summary>Price to analyse.</summary>
    public required MoneyDto Price { get; init; }

    /// <summary>Resale or NewBuild.</summary>
    /// <example>Resale</example>
    public string PurchaseKind { get; init; } = "Resale";

    /// <summary>Property type: Flat, House, Townhouse, Finca, Penthouse, Studio, Garage…</summary>
    public string PropertyType { get; init; } = "Flat";

    /// <summary>FreeMarket, PriceLimited (VPL), ProtectedSpecialRegime (VPO special regime) or AppraisedPrice.</summary>
    public string HousingRegime { get; init; } = "FreeMarket";

    /// <summary>Municipality; decides the island value limit. Unknown names assume Mallorca.</summary>
    /// <example>Inca</example>
    public string? Municipality { get; init; }

    /// <summary>Cadastral reference value, when known; the tax base is the higher of price and this value.</summary>
    public MoneyDto? CadastralReferenceValue { get; init; }

    /// <summary>Renovation budget; a contingency percentage from the business configuration is added.</summary>
    public MoneyDto? RenovationCost { get; init; }

    /// <summary>Stored profile to use; when both this and <c>financing</c> are absent the default profile is used.</summary>
    public Guid? FinancingProfileId { get; init; }

    /// <summary>Inline financing assumptions; take precedence over <c>financingProfileId</c>.</summary>
    public FinancingAssumptionsDto? Financing { get; init; }

    /// <summary>Euríbor 12M in % to use instead of the configured reference.</summary>
    public decimal? EuriborPercent { get; init; }

    /// <summary>Calculation date (rules, island limits and aid valid that day); defaults to today.</summary>
    public DateOnly? On { get; init; }
}

/// <summary>Inverse calculation: highest price the profile can pay.</summary>
public sealed record MaximumPriceRequestDto
{
    /// <summary>Stored profile to use; when both this and <c>financing</c> are absent the default profile is used.</summary>
    public Guid? FinancingProfileId { get; init; }

    /// <summary>Inline financing assumptions; take precedence over <c>financingProfileId</c>.</summary>
    public FinancingAssumptionsDto? Financing { get; init; }

    /// <summary>Resale or NewBuild.</summary>
    public string PurchaseKind { get; init; } = "Resale";

    /// <summary>Property type.</summary>
    public string PropertyType { get; init; } = "Flat";

    /// <summary>Municipality; decides the island value limit.</summary>
    public string? Municipality { get; init; }

    /// <summary>Renovation budget to keep on top of the price.</summary>
    public MoneyDto? RenovationCost { get; init; }

    /// <summary>Use an eligible ICO or IBAVI guarantee when it raises the loan-to-value.</summary>
    public bool IncludeGuarantees { get; init; }

    /// <summary>Affordable (default) or Borderline.</summary>
    public string MinimumVerdict { get; init; } = "Affordable";

    /// <summary>Calculation date; defaults to today.</summary>
    public DateOnly? On { get; init; }
}

/// <summary>Highest affordable price with the analysis at that price.</summary>
public sealed record MaximumPriceResultDto
{
    /// <summary>Configuration versions used.</summary>
    public required AnalysisConfigurationDto Configuration { get; init; }

    /// <summary>Maximum price and what limits it.</summary>
    public required MaximumPriceDto Maximum { get; init; }

    /// <summary>Full analysis at the maximum price; null when there is none.</summary>
    public PurchaseAnalysisDto? AtMaximum { get; init; }
}
