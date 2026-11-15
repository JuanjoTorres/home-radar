namespace HomeRadar.Domain.Finance;

public static class TaxRuleDocumentValidator
{
    public static IReadOnlyList<string> Validate(TaxRuleDocument document)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(document.Jurisdiction))
        {
            errors.Add("jurisdiction is required");
        }

        errors.AddRange(document.Rules.GroupBy(rule => rule.Id, StringComparer.Ordinal).Where(group => group.Count() > 1).Select(group => $"duplicated rule id {group.Key}"));
        errors.AddRange(document.Rules.SelectMany(ValidateRule));
        errors.AddRange(document.Rules
            .GroupBy(rule => (rule.Tax, rule.AppliesTo))
            .Where(group => group.Count(rule => rule.Role == TaxRuleRole.Standard && !rule.IsBonus) != 1)
            .Select(group => $"{group.Key.Tax}/{group.Key.AppliesTo} needs exactly one standard rule"));
        errors.AddRange(document.Fees.SelectMany(ValidateFee));
        errors.AddRange(document.AidPrograms.SelectMany(ValidateAid));
        if (document.Subdivisions.Count > 0 && document.Subdivisions.Count(subdivision => subdivision.IsDefault) != 1)
        {
            errors.Add("exactly one subdivision must be the default");
        }

        errors.AddRange(document.Subdivisions.Where(subdivision => subdivision.ValueLimits.Count == 0).Select(subdivision => $"subdivision {subdivision.Code} has no value limits"));
        return errors;
    }

    private static IEnumerable<string> ValidateRule(TaxRule rule)
    {
        switch (rule.Kind)
        {
            case TaxRuleKind.ProgressiveScale when !BracketsAreAscendingAndOpen(rule.Brackets.Select(bracket => bracket.UpTo).ToList()):
                yield return $"rule {rule.Id}: brackets must ascend and end without upper bound";
                break;
            case TaxRuleKind.ReducedRate or TaxRuleKind.FlatRate when rule.Rate is null or < 0m or > 1m:
                yield return $"rule {rule.Id}: rate must be a fraction between 0 and 1";
                break;
            case TaxRuleKind.QuotaBonus when rule.Bonus is null or <= 0m or > 1m:
                yield return $"rule {rule.Id}: bonus must be a fraction in (0, 1]";
                break;
        }

        if (rule.IsBonus && rule.Role != TaxRuleRole.Relief)
        {
            yield return $"rule {rule.Id}: bonuses must have the Relief role";
        }

        if (rule.Role == TaxRuleRole.Standard && !rule.Conditions.IsUnconditional())
        {
            yield return $"rule {rule.Id}: the standard rule cannot have conditions";
        }

        if (rule.SourceUrl is null)
        {
            yield return $"rule {rule.Id}: sourceUrl is required";
        }
    }

    private static IEnumerable<string> ValidateFee(FeeTariff fee)
    {
        if (fee.Brackets.Count == 0 || fee.Brackets[0].Fixed is null)
        {
            yield return $"fee {fee.Id}: the first bracket must be a fixed amount";
        }

        if (fee.Reduction is < 0m or >= 1m || fee.Vat is < 0m or > 1m)
        {
            yield return $"fee {fee.Id}: reduction and vat must be fractions";
        }
    }

    private static IEnumerable<string> ValidateAid(AidProgram program)
    {
        if (program.Kind == AidKind.LoanGuarantee && (program.GuaranteeTiers.Count == 0 || program.GuaranteeTiers.Any(tier => tier.MaxLoanToValue is <= 0m or > 1m)))
        {
            yield return $"aid {program.Id}: guarantees need tiers with maxLoanToValue in (0, 1]";
        }

        if (program.Kind == AidKind.DirectGrant && program.GrantAmount is null or <= 0m)
        {
            yield return $"aid {program.Id}: grants need a positive grantAmount";
        }
    }

    private static bool BracketsAreAscendingAndOpen(List<decimal?> bounds)
    {
        if (bounds.Count == 0 || bounds[^1] is not null)
        {
            return false;
        }

        var bounded = bounds.Take(bounds.Count - 1).ToList();
        return bounded.All(bound => bound is not null) && bounded.Zip(bounded.Skip(1)).All(pair => pair.First < pair.Second);
    }

}
