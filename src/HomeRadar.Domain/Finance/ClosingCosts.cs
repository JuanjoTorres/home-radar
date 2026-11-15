using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Domain.Finance;

public enum CostCategory
{
    Tax = 0,
    Notary = 1,
    LandRegistry = 2,
    Gestoria = 3,
    Appraisal = 4,
    MortgageOpeningFee = 5,
    GuaranteeFee = 6,
    Renovation = 7,
    RenovationContingency = 8,
}

public sealed record CostLine(CostCategory Category, string Code, Money Amount, string Explanation, bool Estimated, string? SourceUrl);

public static class ClosingCostCalculator
{
    public static IReadOnlyList<CostLine> PurchaseFees(IReadOnlyList<FeeTariff> tariffs, Money price, bool hasMortgage, ClosingCostEstimates estimates)
    {
        var lines = new List<CostLine>();
        foreach (var tariff in tariffs)
        {
            lines.Add(new CostLine(
                tariff.Kind == FeeKind.NotaryDeed ? CostCategory.Notary : CostCategory.LandRegistry,
                tariff.Id,
                Money.Euros(TariffAmount(tariff, price.Amount)),
                tariff.Note is { } note ? $"{tariff.Description ?? tariff.Id}. {note}" : tariff.Description ?? tariff.Id,
                Estimated: !tariff.Verified || tariff.ExtrasEur > 0m,
                tariff.SourceUrl));
        }

        lines.Add(new CostLine(CostCategory.Gestoria, "gestoria", Money.Euros(estimates.GestoriaEur), "Gestoria for the purchase deed (market estimate)", Estimated: true, estimates.SourceUrl));
        if (hasMortgage)
        {
            lines.Add(new CostLine(CostCategory.Appraisal, "appraisal", Money.Euros(estimates.AppraisalEur), "Official appraisal paid by the borrower (Ley 5/2019; market estimate)", Estimated: true, estimates.SourceUrl));
        }

        return lines;
    }

    public static decimal TariffAmount(FeeTariff tariff, decimal taxBase)
    {
        var gross = BracketAmount(tariff.Brackets, taxBase);
        if (tariff.MinEur is { } min)
        {
            gross = Math.Max(gross, min);
        }

        if (tariff.MaxEur is { } max)
        {
            gross = Math.Min(gross, max);
        }

        var net = gross * (1m - tariff.Reduction);
        return (net * (1m + tariff.Vat)) + tariff.ExtrasEur;
    }

    private static decimal BracketAmount(IReadOnlyList<TariffBracket> brackets, decimal taxBase)
    {
        var amount = 0m;
        var lower = 0m;
        foreach (var bracket in brackets)
        {
            if (bracket.Fixed is { } fixedAmount)
            {
                amount += fixedAmount;
            }
            else if (taxBase > lower && bracket.Permille is { } permille)
            {
                var upper = bracket.UpTo is { } upTo ? Math.Min(taxBase, upTo) : taxBase;
                amount += (upper - lower) * permille / 1000m;
            }

            if (bracket.UpTo is not { } next || taxBase <= next)
            {
                break;
            }

            lower = next;
        }

        return amount;
    }
}
