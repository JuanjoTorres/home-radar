using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Domain.Finance;

public sealed record TaxLine(
    TaxKind Tax,
    string RuleId,
    Money TaxBase,
    Money Quota,
    decimal EffectiveRate,
    string Explanation,
    string? BonusRuleId,
    Money BonusAmount,
    string? SourceUrl);

public sealed record ReliefAssessment(
    string RuleId,
    TaxKind Tax,
    EligibilityOutcome Outcome,
    bool Applied,
    IReadOnlyList<ConditionCheck> Checks,
    Money? PotentialSaving,
    string? Description);

public sealed record TaxAssessment(IReadOnlyList<TaxLine> Lines, IReadOnlyList<ReliefAssessment> Reliefs)
{
    public Money Total => Lines.Aggregate(Money.Zero, (sum, line) => sum + line.Quota);
}

public sealed record TaxableEvent(PurchaseKind Purchase, Money Price, Money TaxableValue, DateOnly On);

public static class TaxCalculator
{
    public static TaxAssessment Assess(IReadOnlyList<TaxRule> rules, TaxableEvent purchase, EligibilityFacts facts)
    {
        var lines = new List<TaxLine>();
        var reliefs = new List<ReliefAssessment>();
        foreach (var tax in TaxesFor(purchase.Purchase))
        {
            var applicable = rules.Where(rule => rule.Tax == tax && rule.AppliesTo == purchase.Purchase && rule.IsValidOn(purchase.On)).ToList();
            if (applicable.Count == 0)
            {
                continue;
            }

            var taxBase = tax == TaxKind.ValueAddedTax ? purchase.Price : purchase.TaxableValue;
            var (line, assessed) = AssessTax(tax, applicable, taxBase, facts);
            lines.Add(line);
            reliefs.AddRange(assessed);
        }

        return new TaxAssessment(lines, reliefs);
    }

    public static decimal Quota(TaxRule rule, decimal amount) => rule.Kind switch
    {
        TaxRuleKind.ProgressiveScale => ProgressiveQuota(rule.Brackets, amount),
        TaxRuleKind.ReducedRate => ReducedRateQuota(rule, amount),
        TaxRuleKind.FlatRate => amount * (rule.Rate ?? 0m),
        _ => 0m,
    };

    private static IEnumerable<TaxKind> TaxesFor(PurchaseKind purchase) => purchase == PurchaseKind.Resale
        ? [TaxKind.TransferTax]
        : [TaxKind.ValueAddedTax, TaxKind.StampDuty];

    private static (TaxLine Line, IReadOnlyList<ReliefAssessment> Reliefs) AssessTax(TaxKind tax, IReadOnlyList<TaxRule> rules, Money taxBase, EligibilityFacts facts)
    {
        var evaluated = rules.ToDictionary(rule => rule.Id, rule => EligibilityEvaluator.Evaluate(rule.Conditions, facts), StringComparer.Ordinal);
        var baseRule = SelectBaseRule(rules, evaluated, taxBase.Amount);
        var reliefRates = rules.Where(rule => rule.Role == TaxRuleRole.Relief && !rule.IsBonus).ToList();
        var bonuses = rules.Where(rule => rule.IsBonus).ToList();

        bool IsMet(TaxRule rule) => evaluated[rule.Id].Outcome == EligibilityOutcome.Met;
        var chosen = BestOption(baseRule, reliefRates.Where(IsMet), bonuses.Where(IsMet), taxBase.Amount);

        var assessments = reliefRates.Concat(bonuses)
            .Select(rule => AssessRelief(rule, evaluated[rule.Id], chosen, baseRule, reliefRates, bonuses, IsMet, taxBase.Amount))
            .ToList();

        var quota = Money.Euros(chosen.Quota);
        var line = new TaxLine(
            tax,
            chosen.Rate.Id,
            taxBase,
            quota,
            taxBase.Amount == 0m ? 0m : Math.Round(quota.Amount / taxBase.Amount, 6, MidpointRounding.ToEven),
            Explain(chosen),
            chosen.Bonus?.Id,
            Money.Euros(chosen.BonusAmount),
            chosen.Rate.SourceUrl);
        return (line, assessments);
    }

    private static TaxRule SelectBaseRule(IReadOnlyList<TaxRule> rules, Dictionary<string, EligibilityResult> evaluated, decimal taxBase)
    {
        var overrides = rules.Where(rule => rule.Role == TaxRuleRole.Override && !rule.IsBonus && evaluated[rule.Id].Outcome == EligibilityOutcome.Met).ToList();
        if (overrides.Count > 0)
        {
            return overrides.OrderByDescending(rule => Quota(rule, taxBase)).First();
        }

        return rules.FirstOrDefault(rule => rule.Role == TaxRuleRole.Standard && !rule.IsBonus)
            ?? throw new Common.DomainException($"No standard rule for {rules[0].Tax} {rules[0].AppliesTo}.");
    }

    private static TaxOption BestOption(TaxRule baseRule, IEnumerable<TaxRule> reliefRates, IEnumerable<TaxRule> bonuses, decimal taxBase)
    {
        var bonusList = bonuses.ToList();
        return new[] { baseRule }.Concat(reliefRates)
            .Select(rate => WithBestBonus(rate, bonusList, taxBase))
            .OrderBy(option => option.Quota)
            .ThenBy(option => option.Rate.Role == TaxRuleRole.Relief ? 0 : 1)
            .First();
    }

    private static TaxOption WithBestBonus(TaxRule rate, IReadOnlyList<TaxRule> bonuses, decimal taxBase)
    {
        var gross = Quota(rate, taxBase);
        var best = bonuses
            .Select(bonus => (Bonus: bonus, Amount: (bonus.Bonus ?? 0m) * Quota(rate, Math.Min(taxBase, bonus.BonusAppliesUpTo ?? taxBase))))
            .OrderByDescending(candidate => candidate.Amount)
            .FirstOrDefault();
        return best.Bonus is null || best.Amount <= 0m
            ? new TaxOption(rate, null, gross, 0m)
            : new TaxOption(rate, best.Bonus, gross - best.Amount, best.Amount);
    }

    private static ReliefAssessment AssessRelief(
        TaxRule rule,
        EligibilityResult eligibility,
        TaxOption chosen,
        TaxRule baseRule,
        IReadOnlyList<TaxRule> reliefRates,
        IReadOnlyList<TaxRule> bonuses,
        Func<TaxRule, bool> isMet,
        decimal taxBase)
    {
        var applied = chosen.Rate.Id == rule.Id || chosen.Bonus?.Id == rule.Id;
        Money? saving = null;
        if (eligibility.Outcome == EligibilityOutcome.Unknown)
        {
            bool AssumeMet(TaxRule candidate) => candidate.Id == rule.Id || isMet(candidate);
            var hypothetical = BestOption(baseRule, reliefRates.Where(AssumeMet), bonuses.Where(AssumeMet), taxBase);
            var difference = Money.Euros(chosen.Quota - hypothetical.Quota);
            saving = difference.Amount > 0m ? difference : null;
        }

        return new ReliefAssessment(rule.Id, rule.Tax, eligibility.Outcome, applied, eligibility.Checks, saving, rule.Description);
    }

    private static string Explain(TaxOption option)
    {
        var text = option.Rate.Description ?? option.Rate.Id;
        return option.Bonus is null ? text : $"{text}; {option.Bonus.Description ?? option.Bonus.Id}";
    }

    private static decimal ProgressiveQuota(IReadOnlyList<RateBracket> brackets, decimal amount)
    {
        var quota = 0m;
        var lower = 0m;
        foreach (var bracket in brackets)
        {
            if (amount <= lower)
            {
                break;
            }

            var upper = bracket.UpTo is { } upTo ? Math.Min(amount, upTo) : amount;
            quota += (upper - lower) * bracket.Rate;
            lower = bracket.UpTo ?? amount;
        }

        return quota;
    }

    private static decimal ReducedRateQuota(TaxRule rule, decimal amount)
    {
        var rate = rule.Rate ?? 0m;
        if (rule.RateAppliesUpTo is not { } cap)
        {
            return amount * rate;
        }

        var reduced = Math.Min(amount, cap);
        return (reduced * rate) + (Math.Max(0m, amount - cap) * (rule.ExcessRate ?? rate));
    }

    private sealed record TaxOption(TaxRule Rate, TaxRule? Bonus, decimal Quota, decimal BonusAmount);
}
