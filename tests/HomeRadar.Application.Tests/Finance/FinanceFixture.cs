using HomeRadar.Domain.Finance;
using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.ValueObjects;
using HomeRadar.Infrastructure.Seeding;

namespace HomeRadar.Application.Tests.Finance;

public static class FinanceFixture
{
    public static readonly DateOnly On = new(2026, 11, 15);

    public static string SeedDirectory => Path.Combine(AppContext.BaseDirectory, "seed");

    public static BusinessConfig Config { get; } = YamlSeedLoader.LoadBusinessConfigs(SeedDirectory).OrderByDescending(seed => seed.ValidFrom).ThenByDescending(seed => seed.Revision).First().Config;

    public static IReadOnlyList<TaxRuleDocument> Documents { get; } = YamlSeedLoader.LoadTaxRuleSets(SeedDirectory).Select(seed => seed.Document).OrderBy(document => document.Jurisdiction.Length).ToList();

    public static PurchaseAnalyzer Analyzer { get; } = new(Config, Documents);

    public static BuyerProfile Buyer(
        int age = 34,
        bool firstHome = true,
        int? yearsResident = 5,
        decimal? taxableIncome = null,
        bool? jointReturn = null,
        decimal? grossIncome = null,
        decimal? netWorth = null,
        bool largeFamily = false,
        int? disability = null) =>
        new(age, firstHome, PrimaryResidence: true, ResidentInBalearics: yearsResident is not 0, yearsResident, largeFamily, SingleParentFamily: false, disability, taxableIncome, netWorth, JointTaxReturn: jointReturn, GrossAnnualIncome: grossIncome);

    public static FinancingInputs Financing(
        BuyerProfile? buyer = null,
        decimal savings = 60_000m,
        decimal income = 3_000m,
        decimal debt = 0m,
        decimal loanToValue = 0.8m,
        MortgageTerms? mortgage = null) =>
        new(Money.Euros(savings), Money.Euros(income), Money.Euros(debt), 0.35m, mortgage ?? Fixed(loanToValue), buyer ?? Buyer());

    public static MortgageTerms Fixed(decimal loanToValue = 0.8m, decimal rate = 2.9m, int months = 360) =>
        new(MortgageType.Fixed, loanToValue, rate, null, null, months);

    public static PurchaseScenario Scenario(decimal price, PurchaseKind purchase = PurchaseKind.Resale, string? municipality = "Palma", decimal renovation = 0m, decimal? referenceValue = null) =>
        new(Money.Euros(price), purchase, PropertyType.Flat, HousingRegime.FreeMarket, municipality, referenceValue is { } value ? Money.Euros(value) : null, Money.Euros(renovation), false, On);

    public static PurchaseAnalysis Evaluate(PurchaseScenario scenario, FinancingInputs? financing = null) =>
        Analyzer.Evaluate(scenario, financing ?? Financing(), new LoanOption((financing ?? Financing()).Mortgage.MaxLoanToValue, null, 0m));

    public static TaxLine Tax(PurchaseAnalysis analysis, TaxKind kind) => analysis.Taxes.Lines.Single(line => line.Tax == kind);
}
