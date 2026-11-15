using HomeRadar.Domain.Finance;
using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.ValueObjects;
using static HomeRadar.Application.Tests.Finance.FinanceFixture;

namespace HomeRadar.Application.Tests.Finance;

public sealed class CostAndMortgageTests
{
    [Theory]
    [InlineData(5_000, 253.63, 31.27)]
    [InlineData(250_000, 590.76, 234.93)]
    [InlineData(300_000, 619.50, 252.18)]
    [InlineData(700_000, 826.64, 378.74)]
    public void Notary_and_registry_follow_their_official_tariffs(decimal price, decimal notary, decimal registry)
    {
        var costs = Evaluate(Scenario(price)).Costs;

        costs.Single(cost => cost.Category == CostCategory.Notary).Amount.Amount.ShouldBe(notary);
        costs.Single(cost => cost.Category == CostCategory.LandRegistry).Amount.Amount.ShouldBe(registry);
    }

    [Fact]
    public void Appraisal_is_only_charged_when_there_is_a_mortgage()
    {
        var withLoan = Evaluate(Scenario(250_000));
        var cash = Evaluate(Scenario(250_000), Financing(loanToValue: 0m));

        withLoan.Costs.ShouldContain(cost => cost.Category == CostCategory.Appraisal && cost.Amount.Amount == 450m);
        cash.Costs.ShouldNotContain(cost => cost.Category == CostCategory.Appraisal);
    }

    [Theory]
    [InlineData(200_000, 3.0, 360, 843.21)]
    [InlineData(200_000, 2.9, 360, 832.46)]
    [InlineData(120_000, 0.0, 240, 500.00)]
    public void Fixed_payment_uses_the_french_amortisation_formula(decimal loan, decimal rate, int months, decimal expected)
    {
        var schedule = MortgageCalculator.Schedule(Money.Euros(loan), new MortgageTerms(MortgageType.Fixed, 0.8m, rate, null, null, months), 3.239m, new RateScenario("base", 0m));

        schedule.Periods.ShouldHaveSingleItem().MonthlyPayment.Amount.ShouldBe(expected);
    }

    [Theory]
    [InlineData(0, 932.01)]
    [InlineData(1, 1_046.04)]
    [InlineData(2, 1_166.52)]
    public void Variable_payment_resets_after_the_initial_year_with_euribor_plus_spread(decimal delta, decimal expected)
    {
        var terms = new MortgageTerms(MortgageType.Variable, 0.8m, 2.5m, 0.6m, null, 360);

        var schedule = MortgageCalculator.Schedule(Money.Euros(200_000m), terms, 3.239m, new RateScenario("scenario", delta));

        schedule.Periods[0].MonthlyPayment.Amount.ShouldBe(790.24m);
        schedule.Periods[0].ToMonth.ShouldBe(12);
        schedule.Periods[1].AnnualRatePercent.ShouldBe(3.839m + delta);
        schedule.Periods[1].MonthlyPayment.Amount.ShouldBe(expected);
        schedule.MaxMonthlyPayment.Amount.ShouldBe(expected);
    }

    [Fact]
    public void Mixed_payment_keeps_the_fixed_rate_for_the_fixed_leg()
    {
        var terms = new MortgageTerms(MortgageType.Mixed, 0.8m, 2.2m, 0.7m, 120, 360);

        var schedule = MortgageCalculator.Schedule(Money.Euros(200_000m), terms, 3.239m, new RateScenario("base", 0m));

        schedule.Periods[0].MonthlyPayment.Amount.ShouldBe(759.40m);
        schedule.Periods[1].FromMonth.ShouldBe(121);
        schedule.Periods[1].MonthlyPayment.Amount.ShouldBe(888.12m);
    }

    [Fact]
    public void Incomplete_mortgage_terms_are_rejected()
    {
        var result = Analyzer.Analyze(Scenario(250_000), Financing(mortgage: new MortgageTerms(MortgageType.Mixed, 0.8m, 2.2m, null, null, 360)));

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe("mortgage.mixed");
    }
}
