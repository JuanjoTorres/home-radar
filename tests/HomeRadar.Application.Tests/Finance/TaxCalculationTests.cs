using HomeRadar.Domain.Finance;
using static HomeRadar.Application.Tests.Finance.FinanceFixture;

namespace HomeRadar.Application.Tests.Finance;

public sealed class TaxCalculationTests
{
    [Fact]
    public void Seed_files_are_valid()
    {
        Documents.Select(document => document.Jurisdiction).ShouldBe(["ES", "ES-IB"]);
        foreach (var document in Documents)
        {
            TaxRuleDocumentValidator.Validate(document).ShouldBeEmpty();
        }

        Config.Euribor.TwelveMonthPercent.ShouldBe(3.239m);
    }

    [Theory]
    [InlineData(250_000, 5_000.00, "itp_under36_first_home_2")]
    [InlineData(300_000, 7_790.93, "itp_under36_first_home_2")]
    [InlineData(331_859.70, 10_339.70, "itp_under36_first_home_2")]
    [InlineData(340_000, 27_200.00, "itp_general_scale")]
    public void Under_36_first_home_pays_2_percent_up_to_the_island_limit(decimal price, decimal expected, string rule)
    {
        var line = Tax(Evaluate(Scenario(price)), TaxKind.TransferTax);

        line.Quota.Amount.ShouldBe(expected);
        line.RuleId.ShouldBe(rule);
    }

    [Fact]
    public void Primary_residence_over_36_pays_4_percent_on_the_first_tranche_and_8_on_the_excess()
    {
        var line = Tax(Evaluate(Scenario(300_000), Financing(Buyer(age: 40))), TaxKind.TransferTax);

        line.Quota.Amount.ShouldBe(13_193.95m);
        line.RuleId.ShouldBe("itp_primary_residence_4");
    }

    [Theory]
    [InlineData(600_000, 50_000.00)]
    [InlineData(700_000, 60_000.00)]
    public void Second_home_buyers_pay_the_progressive_scale(decimal price, decimal expected)
    {
        var line = Tax(Evaluate(Scenario(price), Financing(Buyer(age: 45, firstHome: false))), TaxKind.TransferTax);

        line.Quota.Amount.ShouldBe(expected);
        line.RuleId.ShouldBe("itp_general_scale");
    }

    [Fact]
    public void Under_30_with_every_requirement_gets_the_100_percent_bonus_on_the_first_tranche()
    {
        var analysis = Evaluate(Scenario(300_000), Financing(Buyer(age: 28, taxableIncome: 30_000m, jointReturn: false)));
        var line = Tax(analysis, TaxKind.TransferTax);

        line.Quota.Amount.ShouldBe(2_387.90m);
        line.BonusRuleId.ShouldBe("itp_under30_or_disability33_bonus_100");
        analysis.Warnings.ShouldContain(warning => warning.Contains("itp_under30_or_disability33_bonus_100", StringComparison.Ordinal));
    }

    [Fact]
    public void A_relief_that_cannot_be_checked_is_not_applied_but_reports_its_saving()
    {
        var analysis = Evaluate(Scenario(300_000), Financing(Buyer(age: 28)));
        var bonus = analysis.Taxes.Reliefs.Single(relief => relief.RuleId == "itp_under30_or_disability33_bonus_100");

        Tax(analysis, TaxKind.TransferTax).Quota.Amount.ShouldBe(7_790.93m);
        bonus.Outcome.ShouldBe(EligibilityOutcome.Unknown);
        bonus.Applied.ShouldBeFalse();
        bonus.PotentialSaving!.Value.Amount.ShouldBe(5_403.02m);
        bonus.Checks.ShouldContain(check => check.Condition == "taxableIncomeMax" && check.Outcome == EligibilityOutcome.Unknown);
    }

    [Fact]
    public void Taxable_income_between_individual_and_joint_limits_needs_the_type_of_return()
    {
        var unknown = Evaluate(Scenario(300_000), Financing(Buyer(age: 28, taxableIncome: 60_000m)));
        var joint = Evaluate(Scenario(300_000), Financing(Buyer(age: 28, taxableIncome: 60_000m, jointReturn: true)));

        unknown.Taxes.Reliefs.Single(relief => relief.RuleId == "itp_under30_or_disability33_bonus_100").Outcome.ShouldBe(EligibilityOutcome.Unknown);
        Tax(joint, TaxKind.TransferTax).Quota.Amount.ShouldBe(2_387.90m);
    }

    [Fact]
    public void Tax_base_is_the_higher_of_price_and_cadastral_reference_value()
    {
        var line = Tax(Evaluate(Scenario(250_000, referenceValue: 300_000)), TaxKind.TransferTax);

        line.TaxBase.Amount.ShouldBe(300_000m);
        line.Quota.Amount.ShouldBe(7_790.93m);
    }

    [Fact]
    public void Large_families_keep_the_2_percent_up_to_115_percent_of_the_island_limit()
    {
        var line = Tax(Evaluate(Scenario(370_000), Financing(Buyer(age: 45, largeFamily: true))), TaxKind.TransferTax);

        line.RuleId.ShouldBe("itp_large_or_single_parent_family_2");
        line.Quota.Amount.ShouldBe(13_390.93m);
    }

    [Fact]
    public void New_build_under_36_pays_10_percent_vat_and_half_percent_stamp_duty()
    {
        var analysis = Evaluate(Scenario(250_000, PurchaseKind.NewBuild));

        Tax(analysis, TaxKind.ValueAddedTax).Quota.Amount.ShouldBe(25_000m);
        Tax(analysis, TaxKind.StampDuty).Quota.Amount.ShouldBe(1_250m);
        analysis.Taxes.Lines.ShouldNotContain(line => line.Tax == TaxKind.TransferTax);
    }

    [Fact]
    public void New_build_first_home_over_36_pays_1_percent_stamp_duty_on_the_first_tranche()
    {
        var line = Tax(Evaluate(Scenario(300_000, PurchaseKind.NewBuild), Financing(Buyer(age: 45))), TaxKind.StampDuty);

        line.Quota.Amount.ShouldBe(3_149.24m);
        line.RuleId.ShouldBe("ajd_primary_residence_1");
    }

    [Fact]
    public void Stamp_duty_rises_to_2_percent_from_one_million()
    {
        var line = Tax(Evaluate(Scenario(1_200_000, PurchaseKind.NewBuild), Financing(Buyer(age: 50, firstHome: false))), TaxKind.StampDuty);

        line.Quota.Amount.ShouldBe(24_000m);
        line.RuleId.ShouldBe("ajd_high_value_2");
    }

    [Theory]
    [InlineData("Palma", "MALLORCA", false)]
    [InlineData("PALMA DE MALLORCA", "MALLORCA", false)]
    [InlineData("Maó-Mahón", "MENORCA", false)]
    [InlineData("mahon", "MENORCA", false)]
    [InlineData("Santa Eulària des Riu", "EIVISSA", false)]
    [InlineData("Cala Millor", "MALLORCA", true)]
    public void Municipalities_resolve_to_their_island(string municipality, string island, bool assumed)
    {
        var location = Analyzer.ResolveLocation(municipality, On);

        location.SubdivisionCode.ShouldBe(island);
        location.Assumed.ShouldBe(assumed);
    }

    [Fact]
    public void Island_limits_follow_their_validity_dates()
    {
        Analyzer.ResolveLocation("Inca", new DateOnly(2026, 5, 1)).ValueLimit.ShouldBe(307_089.00m);
        Analyzer.ResolveLocation("Inca", On).ValueLimit.ShouldBe(331_859.70m);
        Analyzer.ResolveLocation("Eivissa", On).ValueLimit.ShouldBe(378_211.68m);
    }
}
