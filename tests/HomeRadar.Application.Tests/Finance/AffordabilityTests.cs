using CsCheck;
using HomeRadar.Domain.Finance;
using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.ValueObjects;
using static HomeRadar.Application.Tests.Finance.FinanceFixture;

namespace HomeRadar.Application.Tests.Finance;

public sealed class AffordabilityTests
{
    [Fact]
    public void Hand_checked_purchase_of_250000_is_affordable()
    {
        var analysis = Evaluate(Scenario(250_000));

        analysis.TotalAcquisitionCost.Amount.ShouldBe(256_675.69m);
        analysis.Loan.Amount.ShouldBe(200_000m);
        analysis.CashNeeded.Amount.ShouldBe(56_675.69m);
        analysis.RemainingSavings.Amount.ShouldBe(3_324.31m);
        analysis.Payments.ShouldHaveSingleItem().MaxMonthlyPayment.Amount.ShouldBe(832.46m);
        analysis.DebtToIncome.ShouldBe(0.2775m);
        analysis.Verdict.ShouldBe(AffordabilityVerdict.Affordable);
        analysis.Reasons.ShouldBeEmpty();
    }

    [Fact]
    public void A_small_cash_gap_is_borderline_and_a_large_one_is_not_affordable()
    {
        var borderline = Evaluate(Scenario(250_000), Financing(savings: 54_000m));
        var blocked = Evaluate(Scenario(250_000), Financing(savings: 40_000m));

        borderline.Verdict.ShouldBe(AffordabilityVerdict.Borderline);
        borderline.Reasons.ShouldHaveSingleItem().Code.ShouldBe(VerdictReasonCode.CashShortfall);
        borderline.Reasons[0].Amount!.Value.Amount.ShouldBe(2_675.69m);
        blocked.Verdict.ShouldBe(AffordabilityVerdict.NotAffordable);
        blocked.Reasons[0].Blocking.ShouldBeTrue();
    }

    [Fact]
    public void Debt_to_income_above_the_limit_plus_tolerance_blocks_the_purchase()
    {
        var analysis = Evaluate(Scenario(250_000), Financing(income: 2_000m, savings: 100_000m));

        analysis.DebtToIncome.ShouldBe(0.4162m);
        analysis.Verdict.ShouldBe(AffordabilityVerdict.NotAffordable);
        analysis.Reasons.ShouldHaveSingleItem().Code.ShouldBe(VerdictReasonCode.DebtToIncomeAboveLimit);
    }

    [Fact]
    public void Variable_mortgage_that_fails_the_stress_scenario_is_borderline()
    {
        var variable = new MortgageTerms(MortgageType.Variable, 0.8m, null, 0.6m, null, 360);
        var analysis = Evaluate(Scenario(250_000), Financing(mortgage: variable, income: 3_000m));

        analysis.Payments.Select(schedule => schedule.Scenario).ShouldBe(["base", "plus1", "plus2"]);
        analysis.DebtToIncome.ShouldBeLessThanOrEqualTo(0.35m);
        analysis.StressDebtToIncome.ShouldBeGreaterThan(0.35m);
        analysis.Verdict.ShouldBe(AffordabilityVerdict.Borderline);
        analysis.Reasons.ShouldHaveSingleItem().Code.ShouldBe(VerdictReasonCode.StressScenarioAboveLimit);
    }

    [Fact]
    public void A_loan_ending_after_the_usual_maximum_age_is_borderline()
    {
        var analysis = Evaluate(Scenario(150_000), Financing(buyer: Buyer(age: 50, firstHome: false), savings: 100_000m));

        analysis.AgeAtLoanMaturity.ShouldBe(80);
        analysis.Verdict.ShouldBe(AffordabilityVerdict.Borderline);
        analysis.Reasons.ShouldContain(reason => reason.Code == VerdictReasonCode.LoanEndsAfterMaximumAge);
    }

    [Fact]
    public void Renovation_adds_its_cost_and_the_contingency()
    {
        var analysis = Evaluate(Scenario(250_000, renovation: 20_000m));

        analysis.Costs.Single(cost => cost.Category == CostCategory.Renovation).Amount.Amount.ShouldBe(20_000m);
        analysis.Costs.Single(cost => cost.Category == CostCategory.RenovationContingency).Amount.Amount.ShouldBe(2_000m);
        analysis.TotalAcquisitionCost.Amount.ShouldBe(278_675.69m);
    }

    [Fact]
    public void Ibavi_guarantee_finances_100_percent_and_charges_its_fee()
    {
        var financing = Financing(buyer: Buyer(grossIncome: 45_000m), savings: 10_000m);

        var result = Analyzer.Analyze(Scenario(250_000), financing).Value!;

        result.Base.Verdict.ShouldBe(AffordabilityVerdict.NotAffordable);
        result.WithGuarantee.ShouldNotBeNull();
        result.WithGuarantee.LoanOption.GuaranteeProgramId.ShouldBe("ibavi_nova_hipoteca_jove");
        result.WithGuarantee.Loan.Amount.ShouldBe(250_000m);
        result.WithGuarantee.Costs.Single(cost => cost.Category == CostCategory.GuaranteeFee).Amount.Amount.ShouldBe(250m);
        result.WithGuarantee.CashNeeded.Amount.ShouldBe(6_925.69m);
        result.Aid.Single(aid => aid.ProgramId == "ico_state_guarantee").Outcome.ShouldBe(EligibilityOutcome.Unknown);
        result.Aid.Single(aid => aid.ProgramId == "govern_direct_grant_10000").Outcome.ShouldBe(EligibilityOutcome.Met);
    }

    [Fact]
    public void Ibavi_guarantee_is_lost_above_its_price_cap()
    {
        var result = Analyzer.Analyze(Scenario(390_000), Financing(buyer: Buyer(grossIncome: 45_000m), savings: 10_000m)).Value!;

        result.Aid.Single(aid => aid.ProgramId == "ibavi_nova_hipoteca_jove").Outcome.ShouldBe(EligibilityOutcome.NotMet);
        result.WithGuarantee.ShouldBeNull();
    }

    [Fact]
    public void Maximum_price_is_the_last_affordable_cent()
    {
        var financing = Financing();

        var maximum = Analyzer.MaximumAffordablePrice(Scenario(100_000), financing, useGuarantee: false, AffordabilityVerdict.Affordable).Value!;

        var price = maximum.Price!.Value.Amount;
        Evaluate(Scenario(price), financing).Verdict.ShouldBe(AffordabilityVerdict.Affordable);
        Evaluate(Scenario(price + 0.01m), financing).Verdict.ShouldNotBe(AffordabilityVerdict.Affordable);
        maximum.BindingConstraints.ShouldContain(reason => reason.Code == VerdictReasonCode.CashShortfall);
    }

    [Fact]
    public void Nothing_is_affordable_when_the_loan_always_ends_too_late()
    {
        var maximum = Analyzer.MaximumAffordablePrice(Scenario(100_000), Financing(buyer: Buyer(age: 60, firstHome: false)), useGuarantee: false, AffordabilityVerdict.Affordable).Value!;

        maximum.Price.ShouldBeNull();
        maximum.BindingConstraints.ShouldContain(reason => reason.Code == VerdictReasonCode.LoanEndsAfterMaximumAge);
    }

    [Fact]
    public void Maximum_price_round_trips_through_the_analysis_for_any_profile() =>
        Gen.Select(
                Gen.Int[20, 45],
                Gen.Decimal[5_000m, 250_000m],
                Gen.Decimal[1_200m, 9_000m],
                Gen.Decimal[0m, 800m],
                Gen.OneOf(0.7m, 0.8m, 0.9m, 1.0m),
                Gen.Bool,
                Gen.OneOf("Palma", "Maó", "Eivissa", "Cala Millor"))
            .Sample((age, savings, income, debt, loanToValue, newBuild, municipality) =>
            {
                var financing = Financing(Buyer(age: age, taxableIncome: 30_000m, jointReturn: false), Math.Round(savings, 2), Math.Round(income, 2), Math.Round(debt, 2), loanToValue);
                var scenario = Scenario(100_000, newBuild ? PurchaseKind.NewBuild : PurchaseKind.Resale, municipality);

                var maximum = Analyzer.MaximumAffordablePrice(scenario, financing, useGuarantee: false, AffordabilityVerdict.Affordable).Value!;

                if (maximum.Price is { } price)
                {
                    Analyzer.Evaluate(scenario.WithPrice(price), financing, new LoanOption(loanToValue, null, 0m)).Verdict.ShouldBe(AffordabilityVerdict.Affordable);
                    Analyzer.Evaluate(scenario.WithPrice(price + Money.Euros(0.01m)), financing, new LoanOption(loanToValue, null, 0m)).Verdict.ShouldNotBe(AffordabilityVerdict.Affordable);
                }
            },
            iter: 60);

    [Fact]
    public void Total_cost_never_decreases_when_the_price_rises() =>
        Gen.Select(Gen.Decimal[50_000m, 1_500_000m], Gen.Decimal[0.01m, 5_000m], Gen.Int[20, 60], Gen.Bool)
            .Sample((price, step, age, newBuild) =>
            {
                var financing = Financing(Buyer(age: age));
                var kind = newBuild ? PurchaseKind.NewBuild : PurchaseKind.Resale;
                var lower = Evaluate(Scenario(Math.Round(price, 2), kind), financing).TotalAcquisitionCost;
                var higher = Evaluate(Scenario(Math.Round(price + step, 2), kind), financing).TotalAcquisitionCost;
                higher.ShouldBeGreaterThanOrEqualTo(lower);
            },
            iter: 300);

    [Fact]
    public void Budget_limit_is_the_highest_price_whose_total_cost_fits() =>
        Gen.Decimal[60_000m, 900_000m]
            .Sample(budget =>
            {
                var rounded = Money.Euros(budget);
                var price = Analyzer.MaximumPriceWithinBudget(Scenario(100_000), null, rounded)!.Value;
                var cash = Financing(Buyer(age: 99, firstHome: false, yearsResident: 0), savings: budget, loanToValue: 0m);

                Evaluate(Scenario(price.Amount), cash).TotalAcquisitionCost.ShouldBeLessThanOrEqualTo(rounded);
                Evaluate(Scenario(price.Amount + 0.01m), cash).TotalAcquisitionCost.ShouldBeGreaterThan(rounded);
            },
            iter: 40);
}
