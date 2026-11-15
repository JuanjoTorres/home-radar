using HomeRadar.Domain.Common;
using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Domain.Finance;

public sealed record PaymentPeriod(int FromMonth, int ToMonth, decimal AnnualRatePercent, Money MonthlyPayment);

public sealed record PaymentSchedule(string Scenario, decimal EuriborPercent, IReadOnlyList<PaymentPeriod> Periods, Money MaxMonthlyPayment, Money TotalInterest);

public static class MortgageCalculator
{
    private const int MonthsPerYear = 12;
    private const int DefaultVariableInitialMonths = 12;

    public static DomainError? Validate(MortgageTerms terms)
    {
        if (terms.TermMonths <= 0)
        {
            return new DomainError("mortgage.term", "The mortgage term must be positive.");
        }

        return terms.Type switch
        {
            MortgageType.Fixed when terms.NominalRatePercent is null => new DomainError("mortgage.fixedRate", "A fixed mortgage needs nominalRatePercent."),
            MortgageType.Variable when terms.SpreadPercent is null => new DomainError("mortgage.spread", "A variable mortgage needs spreadPercent over Euríbor."),
            MortgageType.Mixed when terms.NominalRatePercent is null || terms.FixedMonths is null || terms.SpreadPercent is null => new DomainError("mortgage.mixed", "A mixed mortgage needs nominalRatePercent, fixedMonths and spreadPercent."),
            _ => null,
        };
    }

    public static PaymentSchedule Schedule(Money loan, MortgageTerms terms, decimal euriborPercent, RateScenario scenario)
    {
        var scenarioEuribor = euriborPercent + scenario.EuriborDeltaPercent;
        var months = terms.TermMonths;
        var (initialMonths, initialRate) = InitialPeriod(terms);
        var resetRate = Math.Max(0m, scenarioEuribor) + (terms.SpreadPercent ?? 0m);

        if (initialMonths <= 0 || initialMonths >= months)
        {
            var rate = initialMonths >= months ? initialRate : resetRate;
            var payment = MonthlyPayment(loan.Amount, rate, months);
            return Build(scenario.Name, scenarioEuribor, loan, [new(1, months, rate, payment, months)]);
        }

        var firstPayment = MonthlyPayment(loan.Amount, initialRate, months);
        var balance = OutstandingBalance(loan.Amount, initialRate, firstPayment, initialMonths);
        var secondPayment = MonthlyPayment(balance, resetRate, months - initialMonths);
        return Build(scenario.Name, scenarioEuribor, loan, [
            new(1, initialMonths, initialRate, firstPayment, initialMonths),
            new(initialMonths + 1, months, resetRate, secondPayment, months - initialMonths),
        ]);
    }

    public static decimal MonthlyPayment(decimal principal, decimal annualRatePercent, int months)
    {
        if (principal <= 0m || months <= 0)
        {
            return 0m;
        }

        var monthlyRate = annualRatePercent / 100m / MonthsPerYear;
        if (monthlyRate == 0m)
        {
            return principal / months;
        }

        var growth = Power(1m + monthlyRate, months);
        return principal * monthlyRate * growth / (growth - 1m);
    }

    public static decimal OutstandingBalance(decimal principal, decimal annualRatePercent, decimal monthlyPayment, int paidMonths)
    {
        var monthlyRate = annualRatePercent / 100m / MonthsPerYear;
        if (monthlyRate == 0m)
        {
            return Math.Max(0m, principal - (monthlyPayment * paidMonths));
        }

        var growth = Power(1m + monthlyRate, paidMonths);
        return Math.Max(0m, (principal * growth) - (monthlyPayment * (growth - 1m) / monthlyRate));
    }

    private static (int Months, decimal RatePercent) InitialPeriod(MortgageTerms terms) => terms.Type switch
    {
        MortgageType.Fixed => (terms.TermMonths, terms.NominalRatePercent ?? 0m),
        MortgageType.Mixed => (terms.FixedMonths ?? 0, terms.NominalRatePercent ?? 0m),
        _ when terms.NominalRatePercent is { } initial => (terms.FixedMonths ?? DefaultVariableInitialMonths, initial),
        _ => (0, 0m),
    };

    private static PaymentSchedule Build(string scenario, decimal euribor, Money loan, IReadOnlyList<RawPeriod> periods)
    {
        var paid = periods.Sum(period => period.Payment * period.Count);
        var rendered = periods.Select(period => new PaymentPeriod(period.From, period.To, period.Rate, Money.Euros(period.Payment))).ToList();
        return new PaymentSchedule(
            scenario,
            euribor,
            rendered,
            rendered.Max(period => period.MonthlyPayment),
            Money.Euros(Math.Max(0m, paid - loan.Amount)));
    }

    private static decimal Power(decimal value, int exponent)
    {
        var result = 1m;
        var factor = value;
        var remaining = exponent;
        while (remaining > 0)
        {
            if ((remaining & 1) == 1)
            {
                result *= factor;
            }

            factor *= factor;
            remaining >>= 1;
        }

        return result;
    }

    private sealed record RawPeriod(int From, int To, decimal Rate, decimal Payment, int Count);
}
