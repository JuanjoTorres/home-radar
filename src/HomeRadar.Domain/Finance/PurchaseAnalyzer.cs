using System.Globalization;
using HomeRadar.Domain.Common;
using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Domain.Finance;

public sealed class PurchaseAnalyzer
{
    private const decimal MinimumSearchPrice = 1_000m;
    private const decimal InitialSearchPrice = 50_000m;
    private const decimal MaximumSearchPrice = 20_000_000m;
    private const decimal Cent = 0.01m;
    private const int MonthsPerYear = 12;

    private readonly BusinessConfig _config;
    private readonly string _jurisdiction;
    private readonly List<TaxRule> _rules;
    private readonly List<FeeTariff> _fees;
    private readonly List<AidProgram> _aid;
    private readonly List<Subdivision> _subdivisions;

    public PurchaseAnalyzer(BusinessConfig config, IReadOnlyList<TaxRuleDocument> documents)
    {
        _config = config;
        _jurisdiction = documents.Count == 0 ? config.DefaultJurisdiction : documents.OrderByDescending(document => document.Jurisdiction.Length).First().Jurisdiction;
        _rules = documents.SelectMany(document => document.Rules).ToList();
        _fees = documents.SelectMany(document => document.Fees).ToList();
        _aid = documents.SelectMany(document => document.AidPrograms).ToList();
        _subdivisions = documents.SelectMany(document => document.Subdivisions).ToList();
    }

    public Result<FinancialAnalysis> Analyze(PurchaseScenario scenario, FinancingInputs financing, bool includeMaximumPrice = true)
    {
        if (Validate(financing) is { } error)
        {
            return error;
        }

        var baseOption = BaseOption(financing);
        var baseAnalysis = Evaluate(scenario, financing, baseOption);
        var guaranteeOption = GuaranteeOption(scenario, financing);
        var withGuarantee = guaranteeOption is not null && guaranteeOption.LoanToValue > baseOption.LoanToValue
            ? Evaluate(scenario, financing, guaranteeOption)
            : null;

        MaximumPrice? maximum = null;
        MaximumPrice? maximumWithGuarantee = null;
        if (includeMaximumPrice)
        {
            maximum = FindMaximumPrice(scenario, financing, useGuarantee: false, AffordabilityVerdict.Affordable);
            var guaranteed = FindMaximumPrice(scenario, financing, useGuarantee: true, AffordabilityVerdict.Affordable);
            maximumWithGuarantee = guaranteed.Price is { } guaranteedPrice && (maximum.Price is not { } basePrice || guaranteedPrice > basePrice) ? guaranteed : null;
        }

        return new FinancialAnalysis(baseAnalysis, AssessAid(scenario, financing), withGuarantee, maximum, maximumWithGuarantee);
    }

    public Result<MaximumPrice> MaximumAffordablePrice(PurchaseScenario scenario, FinancingInputs financing, bool useGuarantee, AffordabilityVerdict target)
    {
        if (Validate(financing) is { } error)
        {
            return error;
        }

        return FindMaximumPrice(scenario, financing, useGuarantee, target);
    }

    public Money? MaximumRenovationBudget(PurchaseScenario scenario, FinancingInputs financing, AffordabilityVerdict target)
    {
        if (Validate(financing) is not null)
        {
            return null;
        }

        var option = BaseOption(financing);
        bool Acceptable(decimal renovation) => Matches(Evaluate(scenario with { RenovationCost = Money.Euros(renovation) }, financing, option).Verdict, target);
        if (!Acceptable(0m))
        {
            return null;
        }

        var low = 0m;
        var high = 10_000m;
        while (high < MaximumSearchPrice && Acceptable(high))
        {
            low = high;
            high *= 2m;
        }

        while (high - low > 1m)
        {
            var middle = Math.Floor((low + high) / 2m);
            if (Acceptable(middle))
            {
                low = middle;
            }
            else
            {
                high = middle;
            }
        }

        return Money.Euros(low);
    }

    public Money? MaximumPriceWithinBudget(PurchaseScenario scenario, FinancingInputs? financing, Money budget)
    {
        var inputs = financing ?? CashBuyer(budget);
        var option = financing is null ? new LoanOption(0m, null, 0m) : BaseOption(financing);
        var price = SearchMaximum(candidate => Evaluate(scenario.WithPrice(Money.Euros(candidate)), inputs, option).TotalAcquisitionCost <= budget);
        return price is { } found ? Money.Euros(found) : null;
    }

    public PurchaseAnalysis Evaluate(PurchaseScenario scenario, FinancingInputs financing, LoanOption option)
    {
        var location = ResolveLocation(scenario.Municipality, scenario.On);
        var price = scenario.Price;
        var taxableValue = scenario.CadastralReferenceValue is { } reference && reference > price ? reference : price;
        var loan = Money.Euros(price.Amount * option.LoanToValue);
        var facts = new EligibilityFacts(financing.Buyer, price, taxableValue, scenario.PropertyType, scenario.HousingRegime, location.ValueLimit, option.LoanToValue, financing.Mortgage.MaxLoanToValue);

        var taxes = TaxCalculator.Assess(_rules, new TaxableEvent(scenario.Purchase, price, taxableValue, scenario.On), facts);
        var costs = Costs(scenario, financing, option, loan);
        var total = costs.Aggregate(price + taxes.Total, (sum, line) => sum + line.Amount);
        var cashNeeded = total - loan;
        var remaining = financing.Savings - cashNeeded;

        var payments = Payments(loan, financing.Mortgage);
        var basePayment = payments[0].MaxMonthlyPayment;
        var stressPayment = payments.FirstOrDefault(schedule => schedule.Scenario == _config.StressScenario)?.MaxMonthlyPayment ?? payments[^1].MaxMonthlyPayment;
        var debtToIncome = DebtToIncome(basePayment, financing);
        var stressDebtToIncome = DebtToIncome(stressPayment, financing);
        var ageAtMaturity = financing.Buyer.Age + (int)Math.Ceiling(financing.Mortgage.TermMonths / (decimal)MonthsPerYear);

        var reasons = Reasons(financing, remaining, debtToIncome, stressDebtToIncome, ageAtMaturity);
        var verdict = reasons.Any(reason => reason.Blocking)
            ? AffordabilityVerdict.NotAffordable
            : reasons.Count > 0 ? AffordabilityVerdict.Borderline : AffordabilityVerdict.Affordable;

        return new PurchaseAnalysis(
            location,
            scenario.Purchase,
            price,
            taxableValue,
            taxes,
            costs,
            total,
            option,
            loan,
            cashNeeded,
            financing.Savings,
            remaining,
            payments,
            _config.StressScenario,
            debtToIncome,
            stressDebtToIncome,
            ageAtMaturity,
            verdict,
            reasons,
            Warnings(scenario, location, taxes, loan));
    }

    public LocationResolution ResolveLocation(string? municipality, DateOnly on)
    {
        var fallback = _subdivisions.FirstOrDefault(subdivision => subdivision.IsDefault) ?? (_subdivisions.Count > 0 ? _subdivisions[0] : null);
        if (fallback is null)
        {
            return new LocationResolution(_jurisdiction, string.Empty, string.Empty, Assumed: true, null);
        }

        var key = municipality is null ? null : TextKeys.Fold(municipality);
        var match = key is null ? null : _subdivisions.FirstOrDefault(subdivision => subdivision.Municipalities.Any(name => TextKeys.Fold(name) == key));
        var resolved = match ?? fallback;
        return new LocationResolution(_jurisdiction, resolved.Code, resolved.Name, Assumed: match is null, resolved.ValueLimitOn(on)?.Amount);
    }

    public IReadOnlyList<Subdivision> Subdivisions => _subdivisions;

    private static DomainError? Validate(FinancingInputs financing)
    {
        if (MortgageCalculator.Validate(financing.Mortgage) is { } mortgageError)
        {
            return mortgageError;
        }

        if (financing.NetMonthlyIncome.Amount <= 0m)
        {
            return new DomainError("financing.income", "Net monthly income must be positive to compute the debt-to-income ratio.");
        }

        return financing.Mortgage.MaxLoanToValue is < 0m or > 1m
            ? new DomainError("financing.loanToValue", "maxLoanToValue must be between 0 and 1.")
            : null;
    }

    private static LoanOption BaseOption(FinancingInputs financing) => new(financing.Mortgage.MaxLoanToValue, null, 0m);

    public static FinancingInputs CashBuyer(Money budget) => new(
        budget,
        Money.Euros(1m),
        Money.Zero,
        1m,
        new MortgageTerms(MortgageType.Fixed, 0m, 0m, null, null, MonthsPerYear),
        new BuyerProfile(99, FirstHome: false, PrimaryResidence: false, ResidentInBalearics: false, null, LargeFamily: false, SingleParentFamily: false, null, null, null, OwnsOtherHome: true));

    private static decimal DebtToIncome(Money payment, FinancingInputs financing) =>
        Math.Round((payment.Amount + financing.ExistingMonthlyDebt.Amount) / financing.NetMonthlyIncome.Amount, 4, MidpointRounding.ToEven);

    private static bool Matches(AffordabilityVerdict verdict, AffordabilityVerdict target) => target == AffordabilityVerdict.Affordable
        ? verdict == AffordabilityVerdict.Affordable
        : verdict != AffordabilityVerdict.NotAffordable;

    private static decimal? SearchMaximum(Func<decimal, bool> acceptable)
    {
        if (!acceptable(MinimumSearchPrice))
        {
            return null;
        }

        var low = MinimumSearchPrice;
        var high = InitialSearchPrice;
        while (high < MaximumSearchPrice && acceptable(high))
        {
            low = high;
            high *= 2m;
        }

        if (high >= MaximumSearchPrice)
        {
            if (acceptable(MaximumSearchPrice))
            {
                return MaximumSearchPrice;
            }

            high = MaximumSearchPrice;
        }

        while (high - low > Cent)
        {
            var middle = Math.Round((low + high) / 2m, 2, MidpointRounding.ToZero);
            if (acceptable(middle))
            {
                low = middle;
            }
            else
            {
                high = middle;
            }
        }

        return low;
    }

    private MaximumPrice FindMaximumPrice(PurchaseScenario scenario, FinancingInputs financing, bool useGuarantee, AffordabilityVerdict target)
    {
        PurchaseAnalysis At(decimal candidate)
        {
            var priced = scenario.WithPrice(Money.Euros(candidate));
            var option = useGuarantee ? GuaranteeOption(priced, financing) ?? BaseOption(financing) : BaseOption(financing);
            return Evaluate(priced, financing, option);
        }

        var maximum = SearchMaximum(candidate => Matches(At(candidate).Verdict, target));
        var limiting = At(maximum is { } found ? found + Cent : MinimumSearchPrice);
        return new MaximumPrice(maximum is { } price ? Money.Euros(price) : null, target, limiting.Reasons);
    }

    private LoanOption? GuaranteeOption(PurchaseScenario scenario, FinancingInputs financing)
    {
        var location = ResolveLocation(scenario.Municipality, scenario.On);
        var taxableValue = scenario.CadastralReferenceValue is { } reference && reference > scenario.Price ? reference : scenario.Price;
        var facts = new EligibilityFacts(financing.Buyer, scenario.Price, taxableValue, scenario.PropertyType, scenario.HousingRegime, location.ValueLimit, financing.Mortgage.MaxLoanToValue, financing.Mortgage.MaxLoanToValue);
        return _aid
            .Where(program => program.Kind == AidKind.LoanGuarantee && program.IsValidOn(scenario.On))
            .Where(program => EligibilityEvaluator.Evaluate(program.Conditions, facts).Outcome == EligibilityOutcome.Met)
            .Select(program => (Program: program, Tier: program.GuaranteeTiers.FirstOrDefault(tier => EligibilityEvaluator.Evaluate(tier.Conditions, facts).Outcome == EligibilityOutcome.Met)))
            .Where(candidate => candidate.Tier is not null)
            .OrderByDescending(candidate => candidate.Tier!.MaxLoanToValue)
            .ThenBy(candidate => candidate.Program.GuaranteeFeePercent)
            .Select(candidate => new LoanOption(Math.Max(candidate.Tier!.MaxLoanToValue, financing.Mortgage.MaxLoanToValue), candidate.Program.Id, candidate.Program.GuaranteeFeePercent))
            .FirstOrDefault();
    }

    private List<AidAssessment> AssessAid(PurchaseScenario scenario, FinancingInputs financing)
    {
        var location = ResolveLocation(scenario.Municipality, scenario.On);
        var taxableValue = scenario.CadastralReferenceValue is { } reference && reference > scenario.Price ? reference : scenario.Price;
        var facts = new EligibilityFacts(financing.Buyer, scenario.Price, taxableValue, scenario.PropertyType, scenario.HousingRegime, location.ValueLimit, financing.Mortgage.MaxLoanToValue, financing.Mortgage.MaxLoanToValue);
        return _aid
            .Where(program => program.IsValidOn(scenario.On))
            .Select(program =>
            {
                var eligibility = EligibilityEvaluator.Evaluate(program.Conditions, facts);
                var tier = program.GuaranteeTiers.FirstOrDefault(candidate => EligibilityEvaluator.Evaluate(candidate.Conditions, facts).Outcome == EligibilityOutcome.Met);
                return new AidAssessment(
                    program.Id,
                    program.Name,
                    program.Kind,
                    eligibility.Outcome,
                    eligibility.Checks,
                    program.Kind == AidKind.LoanGuarantee ? tier?.MaxLoanToValue : null,
                    program.GrantAmount is { } grant ? Money.Euros(grant) : null,
                    program.BudgetLimited,
                    program.Verified,
                    program.SourceUrl,
                    program.Note);
            })
            .ToList();
    }

    private List<CostLine> Costs(PurchaseScenario scenario, FinancingInputs financing, LoanOption option, Money loan)
    {
        var costs = ClosingCostCalculator.PurchaseFees(_fees, scenario.Price, loan.Amount > 0m, _config.ClosingCosts).ToList();
        if (financing.Mortgage.OpeningFeePercent is { } openingPercent && openingPercent > 0m && loan.Amount > 0m)
        {
            costs.Add(new CostLine(CostCategory.MortgageOpeningFee, "mortgageOpeningFee", loan * (openingPercent / 100m), $"Mortgage opening fee {Format(openingPercent)} % of the loan", Estimated: false, null));
        }

        if (option.GuaranteeProgramId is { } programId && option.GuaranteeFeePercent > 0m)
        {
            var guaranteed = scenario.Price * Math.Max(0m, option.LoanToValue - financing.Mortgage.MaxLoanToValue);
            costs.Add(new CostLine(CostCategory.GuaranteeFee, programId, guaranteed * (option.GuaranteeFeePercent / 100m), $"Guarantee processing fee {Format(option.GuaranteeFeePercent)} % of the guaranteed amount", Estimated: false, null));
        }

        if (scenario.RenovationCost.Amount > 0m)
        {
            costs.Add(new CostLine(CostCategory.Renovation, "renovation", scenario.RenovationCost, scenario.RenovationEstimated ? "Estimated renovation cost" : "Renovation budget provided", scenario.RenovationEstimated, null));
            var contingencyPercent = _config.Affordability.RenovationContingencyPercent;
            costs.Add(new CostLine(CostCategory.RenovationContingency, "renovationContingency", scenario.RenovationCost * (contingencyPercent / 100m), $"Contingency {Format(contingencyPercent)} % of the renovation", Estimated: true, null));
        }

        return costs;
    }

    private List<PaymentSchedule> Payments(Money loan, MortgageTerms terms)
    {
        var scenarios = terms.Type == MortgageType.Fixed ? _config.RateScenarios.Take(1) : _config.RateScenarios;
        return scenarios.Select(scenario => MortgageCalculator.Schedule(loan, terms, _config.Euribor.TwelveMonthPercent, scenario)).ToList();
    }

    private List<VerdictReason> Reasons(FinancingInputs financing, Money remaining, decimal debtToIncome, decimal stressDebtToIncome, int ageAtMaturity)
    {
        var thresholds = _config.Affordability;
        var reasons = new List<VerdictReason>();
        if (remaining.Amount < 0m)
        {
            var shortfall = Money.Euros(-remaining.Amount);
            reasons.Add(new VerdictReason(
                VerdictReasonCode.CashShortfall,
                shortfall.Amount > thresholds.BorderlineCashShortfallMax,
                $"Missing {FormatMoney(shortfall.Amount)} EUR of savings for the deposit, taxes and costs",
                shortfall,
                null));
        }

        var limit = financing.MaxDebtToIncomeRatio;
        if (debtToIncome > limit)
        {
            reasons.Add(new VerdictReason(
                VerdictReasonCode.DebtToIncomeAboveLimit,
                debtToIncome > limit + thresholds.BorderlineDebtToIncomeTolerance,
                $"Debt-to-income {Format(debtToIncome * 100m)} % exceeds the {Format(limit * 100m)} % limit",
                null,
                debtToIncome));
        }
        else if (stressDebtToIncome > limit)
        {
            reasons.Add(new VerdictReason(
                VerdictReasonCode.StressScenarioAboveLimit,
                false,
                $"Under the {_config.StressScenario} Euríbor scenario the debt-to-income rises to {Format(stressDebtToIncome * 100m)} %",
                null,
                stressDebtToIncome));
        }

        if (ageAtMaturity > thresholds.MaxAgeAtLoanMaturity)
        {
            reasons.Add(new VerdictReason(
                VerdictReasonCode.LoanEndsAfterMaximumAge,
                false,
                $"The loan ends at age {ageAtMaturity}; banks usually cap it at {thresholds.MaxAgeAtLoanMaturity}, so the term may be shortened",
                null,
                null));
        }

        return reasons;
    }

    private List<string> Warnings(PurchaseScenario scenario, LocationResolution location, TaxAssessment taxes, Money loan)
    {
        var warnings = new List<string>();
        if (location.Assumed)
        {
            warnings.Add(scenario.Municipality is null
                ? $"Municipality unknown; assumed {location.SubdivisionName} for the island value limit"
                : $"Municipality '{scenario.Municipality}' not recognised; assumed {location.SubdivisionName} for the island value limit");
        }

        if (scenario.CadastralReferenceValue is null)
        {
            warnings.Add("Cadastral reference value unknown; the tax base uses the price (the real base is the higher of both)");
        }

        if (scenario.RenovationCost.Amount == 0m)
        {
            warnings.Add("No renovation cost included");
        }

        if (loan.Amount > 0m)
        {
            warnings.Add("Loan-to-value applied over the price; the bank uses the lower of price and appraisal");
        }

        var unverified = taxes.Lines
            .SelectMany(line => new[] { line.RuleId, line.BonusRuleId })
            .OfType<string>()
            .Select(id => _rules.FirstOrDefault(rule => rule.Id == id))
            .Where(rule => rule is { Verified: false });
        warnings.AddRange(unverified.Select(rule => $"Rule {rule!.Id} is an interpretation pending official confirmation: {rule.Note}"));
        warnings.AddRange(_fees.Where(fee => !fee.Verified).Select(fee => $"Fee {fee.Id} is not verified against the official tariff: {fee.Note}"));
        return warnings;
    }

    private static string Format(decimal value) => value.ToString("#,0.##", CultureInfo.InvariantCulture);

    private static string FormatMoney(decimal value) => value.ToString("#,0.00", CultureInfo.InvariantCulture);
}
