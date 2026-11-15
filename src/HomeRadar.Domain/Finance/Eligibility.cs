using System.Globalization;
using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Domain.Finance;

public enum EligibilityOutcome
{
    Met = 0,
    NotMet = 1,
    Unknown = 2,
}

public enum FamilyType
{
    Large = 0,
    SingleParent = 1,
}

public enum HousingRegime
{
    FreeMarket = 0,
    PriceLimited = 1,
    ProtectedSpecialRegime = 2,
    AppraisedPrice = 3,
}

public enum CapBasis
{
    TaxableValue = 0,
    Price = 1,
}

public sealed record PriceCap
{
    public decimal? Amount { get; init; }

    public decimal? SubdivisionLimitFactor { get; init; }

    public CapBasis Basis { get; init; } = CapBasis.TaxableValue;
}

public sealed record EligibilityConditions
{
    public int? AgeMax { get; init; }

    public int? AgeMin { get; init; }

    public bool? FirstHome { get; init; }

    public bool? NoOtherHome { get; init; }

    public bool? PrimaryResidence { get; init; }

    public int? BalearicResidencyYearsMin { get; init; }

    public int? SpanishResidencyYearsMin { get; init; }

    public IReadOnlyList<FamilyType> FamilyTypes { get; init; } = [];

    public int? DisabilityPercentMin { get; init; }

    public bool? DisabilityMinimumForRelatives { get; init; }

    public decimal? TaxableIncomeMaxIndividual { get; init; }

    public decimal? TaxableIncomeMaxJoint { get; init; }

    public decimal? HouseholdIncomeMax { get; init; }

    public decimal? HouseholdIncomeMaxPerBuyer { get; init; }

    public int? BuyerCountMin { get; init; }

    public int? BuyerCountMax { get; init; }

    public decimal? NetWorthMax { get; init; }

    public bool? HasDependentChildren { get; init; }

    public PriceCap? PriceMax { get; init; }

    public decimal? PriceMin { get; init; }

    public decimal? MortgageMinLoanToValue { get; init; }

    public decimal? BankLoanToValueMin { get; init; }

    public IReadOnlyList<HousingRegime> HousingRegimes { get; init; } = [];

    public IReadOnlyList<PropertyType> PropertyTypes { get; init; } = [];

    public IReadOnlyList<EligibilityConditions> AnyOf { get; init; } = [];

    public bool IsUnconditional() =>
        AgeMax is null && AgeMin is null && FirstHome is null && NoOtherHome is null && PrimaryResidence is null
        && BalearicResidencyYearsMin is null && SpanishResidencyYearsMin is null && FamilyTypes.Count == 0
        && DisabilityPercentMin is null && DisabilityMinimumForRelatives is null
        && TaxableIncomeMaxIndividual is null && TaxableIncomeMaxJoint is null && HouseholdIncomeMax is null
        && HouseholdIncomeMaxPerBuyer is null && BuyerCountMin is null && BuyerCountMax is null && NetWorthMax is null
        && HasDependentChildren is null && PriceMax is null && PriceMin is null && MortgageMinLoanToValue is null
        && BankLoanToValueMin is null && HousingRegimes.Count == 0 && PropertyTypes.Count == 0 && AnyOf.Count == 0;
}

public sealed record EligibilityFacts(
    BuyerProfile Buyer,
    Money Price,
    Money TaxableValue,
    PropertyType PropertyType,
    HousingRegime HousingRegime,
    decimal? SubdivisionValueLimit,
    decimal LoanToValue,
    decimal BankLoanToValue);

public sealed record ConditionCheck(string Condition, EligibilityOutcome Outcome, string Detail);

public sealed record EligibilityResult(EligibilityOutcome Outcome, IReadOnlyList<ConditionCheck> Checks)
{
    public IEnumerable<ConditionCheck> Unmet => Checks.Where(check => check.Outcome != EligibilityOutcome.Met);
}

public static class EligibilityEvaluator
{
    public static EligibilityResult Evaluate(EligibilityConditions conditions, EligibilityFacts facts)
    {
        var checks = new List<ConditionCheck>();
        AddBuyerChecks(conditions, facts.Buyer, checks);
        AddIncomeChecks(conditions, facts.Buyer, checks);
        AddPurchaseChecks(conditions, facts, checks);
        if (conditions.AnyOf.Count > 0)
        {
            checks.Add(EvaluateAnyOf(conditions.AnyOf, facts));
        }

        return new EligibilityResult(Combine(checks.Select(check => check.Outcome)), checks);
    }

    private static void AddBuyerChecks(EligibilityConditions conditions, BuyerProfile buyer, List<ConditionCheck> checks)
    {
        if (conditions.AgeMax is { } ageMax)
        {
            checks.Add(Check("ageMax", buyer.Age <= ageMax, $"age {buyer.Age}, required at most {ageMax}"));
        }

        if (conditions.AgeMin is { } ageMin)
        {
            checks.Add(Check("ageMin", buyer.Age >= ageMin, $"age {buyer.Age}, required at least {ageMin}"));
        }

        if (conditions.FirstHome is { } firstHome)
        {
            checks.Add(Check("firstHome", buyer.FirstHome == firstHome, buyer.FirstHome ? "first home" : "not the first home"));
        }

        if (conditions.NoOtherHome is { } noOtherHome)
        {
            checks.Add(Check("noOtherHome", buyer.HasNoOtherHome() is { } hasNone ? hasNone == noOtherHome : null, buyer.HasNoOtherHome() switch
            {
                true => "owns no other home",
                false => "owns another home",
                null => "unknown whether the buyer owns another home",
            }));
        }

        if (conditions.PrimaryResidence is { } primary)
        {
            checks.Add(Check("primaryResidence", buyer.PrimaryResidence == primary, buyer.PrimaryResidence ? "primary residence" : "not the primary residence"));
        }

        if (conditions.BalearicResidencyYearsMin is { } balearicYears)
        {
            var years = buyer.BalearicResidencyYears();
            checks.Add(Check("balearicResidencyYearsMin", years is { } known ? known >= balearicYears : null, years is { } y ? $"{y} years resident in the Balearic Islands, required {balearicYears}" : $"years of residence in the Balearic Islands not provided, required {balearicYears}"));
        }

        if (conditions.SpanishResidencyYearsMin is { } spanishYears)
        {
            checks.Add(SpanishResidency(buyer, spanishYears));
        }

        if (conditions.FamilyTypes.Count > 0)
        {
            var isLarge = conditions.FamilyTypes.Contains(FamilyType.Large) && buyer.LargeFamily;
            var isSingleParent = conditions.FamilyTypes.Contains(FamilyType.SingleParent) && buyer.SingleParentFamily;
            checks.Add(Check("familyTypes", isLarge || isSingleParent, $"family type required: {string.Join(" or ", conditions.FamilyTypes)}"));
        }

        if (conditions.DisabilityPercentMin is { } disability)
        {
            var percent = buyer.DisabilityPercent ?? 0;
            checks.Add(Check("disabilityPercentMin", percent >= disability, $"disability {percent} %, required {disability} %"));
        }

        if (conditions.DisabilityMinimumForRelatives is { } relatives)
        {
            checks.Add(Check("disabilityMinimumForRelatives", buyer.DisabilityMinimumForRelatives == relatives, "IRPF disability minimum for ascendants or descendants"));
        }

        if (conditions.HasDependentChildren is { } children)
        {
            checks.Add(Check("hasDependentChildren", buyer.HasDependentChildren == children, buyer.HasDependentChildren ? "has dependent children" : "no dependent children"));
        }

        if (conditions.BuyerCountMin is { } countMin)
        {
            checks.Add(Check("buyerCountMin", buyer.BuyerCount >= countMin, $"{buyer.BuyerCount} buyer(s), required at least {countMin}"));
        }

        if (conditions.BuyerCountMax is { } countMax)
        {
            checks.Add(Check("buyerCountMax", buyer.BuyerCount <= countMax, $"{buyer.BuyerCount} buyer(s), required at most {countMax}"));
        }
    }

    private static void AddIncomeChecks(EligibilityConditions conditions, BuyerProfile buyer, List<ConditionCheck> checks)
    {
        if (conditions.TaxableIncomeMaxIndividual is not null || conditions.TaxableIncomeMaxJoint is not null)
        {
            checks.Add(TaxableIncome(conditions, buyer));
        }

        if (conditions.HouseholdIncomeMax is { } householdMax)
        {
            checks.Add(Check("householdIncomeMax", buyer.GrossAnnualIncome is { } income ? income <= householdMax : null, buyer.GrossAnnualIncome is { } i ? $"gross household income {Eur(i)}, limit {Eur(householdMax)}" : $"gross household income not provided, limit {Eur(householdMax)}"));
        }

        if (conditions.HouseholdIncomeMaxPerBuyer is { } perBuyer)
        {
            var limit = perBuyer * buyer.BuyerCount;
            checks.Add(Check("householdIncomeMaxPerBuyer", buyer.GrossAnnualIncome is { } income ? income <= limit : null, buyer.GrossAnnualIncome is { } i ? $"gross household income {Eur(i)}, limit {Eur(limit)} ({Eur(perBuyer)} per buyer)" : $"gross household income not provided, limit {Eur(limit)}"));
        }

        if (conditions.NetWorthMax is { } netWorthMax)
        {
            checks.Add(Check("netWorthMax", buyer.NetWorth is { } worth ? worth <= netWorthMax : null, buyer.NetWorth is { } w ? $"net worth {Eur(w)}, limit {Eur(netWorthMax)}" : $"net worth not provided, limit {Eur(netWorthMax)}"));
        }
    }

    private static void AddPurchaseChecks(EligibilityConditions conditions, EligibilityFacts facts, List<ConditionCheck> checks)
    {
        if (conditions.PriceMax is { } cap)
        {
            checks.Add(PriceCapCheck(cap, facts));
        }

        if (conditions.PriceMin is { } priceMin)
        {
            checks.Add(Check("priceMin", facts.TaxableValue.Amount >= priceMin, $"value {Eur(facts.TaxableValue.Amount)}, threshold {Eur(priceMin)}"));
        }

        if (conditions.MortgageMinLoanToValue is { } minLtv)
        {
            checks.Add(Check("mortgageMinLoanToValue", facts.LoanToValue >= minLtv, $"loan {Percent(facts.LoanToValue)} of the price (appraisal assumed equal to price), required {Percent(minLtv)}"));
        }

        if (conditions.BankLoanToValueMin is { } bankLtv)
        {
            checks.Add(Check("bankLoanToValueMin", facts.BankLoanToValue >= bankLtv, $"bank finances {Percent(facts.BankLoanToValue)}, required {Percent(bankLtv)}"));
        }

        if (conditions.HousingRegimes.Count > 0)
        {
            checks.Add(Check("housingRegimes", conditions.HousingRegimes.Contains(facts.HousingRegime), $"housing regime {facts.HousingRegime}, required {string.Join(" or ", conditions.HousingRegimes)}"));
        }

        if (conditions.PropertyTypes.Count > 0)
        {
            checks.Add(Check("propertyTypes", conditions.PropertyTypes.Contains(facts.PropertyType), $"property type {facts.PropertyType}, required {string.Join(" or ", conditions.PropertyTypes)}"));
        }
    }

    private static ConditionCheck SpanishResidency(BuyerProfile buyer, int required)
    {
        if (buyer.YearsResidentInSpain is { } spain)
        {
            return Check("spanishResidencyYearsMin", spain >= required, $"{spain} years resident in Spain, required {required}");
        }

        return buyer.BalearicResidencyYears() is { } balearics && balearics >= required
            ? Check("spanishResidencyYearsMin", true, $"at least {balearics} years resident in Spain (Balearic residence), required {required}")
            : Check("spanishResidencyYearsMin", null, $"years of residence in Spain not provided, required {required}");
    }

    private static ConditionCheck TaxableIncome(EligibilityConditions conditions, BuyerProfile buyer)
    {
        if (buyer.TaxableIncomeBase is not { } income)
        {
            return Check("taxableIncomeMax", null, "IRPF taxable base not provided");
        }

        var individual = conditions.TaxableIncomeMaxIndividual;
        var joint = conditions.TaxableIncomeMaxJoint;
        return buyer.JointTaxReturn switch
        {
            true => Check("taxableIncomeMax", income <= (joint ?? individual), $"IRPF taxable base {Eur(income)}, joint limit {Eur(joint ?? individual ?? 0)}"),
            false => Check("taxableIncomeMax", income <= (individual ?? joint), $"IRPF taxable base {Eur(income)}, individual limit {Eur(individual ?? joint ?? 0)}"),
            null when individual is { } limit && income <= limit => Check("taxableIncomeMax", true, $"IRPF taxable base {Eur(income)}, individual limit {Eur(limit)}"),
            null when joint is { } jointLimit && income <= jointLimit => Check("taxableIncomeMax", null, $"IRPF taxable base {Eur(income)} is within the joint limit {Eur(jointLimit)} only; state whether the return is joint"),
            null => Check("taxableIncomeMax", false, $"IRPF taxable base {Eur(income)} above the limits"),
        };
    }

    private static ConditionCheck PriceCapCheck(PriceCap cap, EligibilityFacts facts)
    {
        var value = cap.Basis == CapBasis.Price ? facts.Price.Amount : facts.TaxableValue.Amount;
        var limit = cap.Amount ?? (cap.SubdivisionLimitFactor is { } factor && facts.SubdivisionValueLimit is { } subdivisionLimit ? Money.Round(subdivisionLimit * factor) : null);
        var label = cap.Basis == CapBasis.Price ? "price" : "taxable value";
        return limit is { } known
            ? Check("priceMax", value <= known, $"{label} {Eur(value)}, limit {Eur(known)}")
            : Check("priceMax", null, $"{label} limit unknown for this location");
    }

    private static ConditionCheck EvaluateAnyOf(IReadOnlyList<EligibilityConditions> alternatives, EligibilityFacts facts)
    {
        var results = alternatives.Select(alternative => Evaluate(alternative, facts)).ToList();
        var outcome = results.Any(result => result.Outcome == EligibilityOutcome.Met)
            ? EligibilityOutcome.Met
            : results.All(result => result.Outcome == EligibilityOutcome.NotMet) ? EligibilityOutcome.NotMet : EligibilityOutcome.Unknown;
        var detail = string.Join(" | ", results.Select(result => string.Join(", ", result.Checks.Select(check => $"{check.Detail} [{check.Outcome}]"))));
        return new ConditionCheck("anyOf", outcome, detail);
    }

    private static EligibilityOutcome Combine(IEnumerable<EligibilityOutcome> outcomes)
    {
        var list = outcomes.ToList();
        if (list.Contains(EligibilityOutcome.NotMet))
        {
            return EligibilityOutcome.NotMet;
        }

        return list.Contains(EligibilityOutcome.Unknown) ? EligibilityOutcome.Unknown : EligibilityOutcome.Met;
    }

    private static ConditionCheck Check(string condition, bool? met, string detail) => new(
        condition,
        met switch { true => EligibilityOutcome.Met, false => EligibilityOutcome.NotMet, null => EligibilityOutcome.Unknown },
        detail);

    private static string Eur(decimal amount) => amount.ToString("#,0.00", CultureInfo.InvariantCulture) + " EUR";

    private static string Percent(decimal fraction) => (fraction * 100m).ToString("0.##", CultureInfo.InvariantCulture) + " %";
}
