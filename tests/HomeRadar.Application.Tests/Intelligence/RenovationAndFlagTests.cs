using HomeRadar.Application.Tests.Finance;
using HomeRadar.Domain.Intelligence;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Application.Tests.Intelligence;

public sealed class RenovationAndFlagTests
{
    private static readonly RenovationConfig Renovation = FinanceFixture.Config.Renovation;
    private static readonly RedFlagConfig Flags = FinanceFixture.Config.RedFlags;

    private static RenovationEstimate Estimate(string? description, int? year = null, decimal? area = 80m, bool newBuild = false, RenovationOverride? manual = null) =>
        new RuleBasedRenovationEstimator().Estimate(new RenovationInput("Piso", description, area, 1, 2, year, newBuild, PropertyType.Flat, manual, 2026), Renovation);

    [Fact]
    public void Phase_three_configuration_is_seeded()
    {
        Renovation.Levels.Count.ShouldBe(4);
        Renovation.Items.Count.ShouldBeGreaterThanOrEqualTo(10);
        Flags.Rules.Count.ShouldBeGreaterThanOrEqualTo(20);
        FinanceFixture.Config.Negotiation.MaxMarginPercent.ShouldBe(15m);
        FinanceFixture.Config.Affordability.RenovationContingencyPercent.ShouldBe(15m);
    }

    [Fact]
    public void Old_flat_to_renovate_is_a_full_renovation_priced_by_the_level_table()
    {
        var estimate = Estimate("Piso a reformar en el centro de Inca", 1975);

        estimate.Level.ShouldBe(RenovationLevel.Full);
        estimate.Confidence.ShouldBe(EstimateConfidence.High);
        estimate.Cost.Probable.ShouldBe(90_728m);
        estimate.Cost.Min.ShouldBe(65_984m);
        estimate.Cost.Max.ShouldBe(131_968m);
        estimate.Lines.Single(line => line.Code == "otherWorks").Cost.Probable.ShouldBe(46_900m);
        estimate.Lines.Single(line => line.Code == "permitsAndTax").Cost.Probable.ShouldBe(2_728m);
        estimate.Lines.Single(line => line.Code == "window").Quantity.ShouldBe(4m);
        estimate.VisitChecklist.ShouldContain(check => check.Contains("Aluminosis", StringComparison.Ordinal));
    }

    [Fact]
    public void Negated_phrases_do_not_count()
    {
        var estimate = Estimate("No necesita reforma, para entrar a vivir", 2015);

        estimate.Level.ShouldBe(RenovationLevel.ReadyToMoveIn);
        estimate.Cost.Probable.ShouldBe(0m);
        estimate.Lines.ShouldBeEmpty();
    }

    [Fact]
    public void Recent_renovation_in_an_old_building_is_light()
    {
        var estimate = Estimate("Totalmente reformado en 2021, cocina nueva", 1968);

        estimate.Level.ShouldBe(RenovationLevel.Light);
        estimate.Signals.ShouldContain(signal => signal.Contains("2021", StringComparison.Ordinal));
    }

    [Fact]
    public void Without_text_the_construction_year_decides()
    {
        Estimate(null, 1995).Level.ShouldBe(RenovationLevel.Medium);
        Estimate(null, 2005).Level.ShouldBe(RenovationLevel.Light);
        Estimate(null, 2015).Level.ShouldBe(RenovationLevel.ReadyToMoveIn);
        var unknown = Estimate(null, null, null);
        unknown.Level.ShouldBe(RenovationLevel.Light);
        unknown.Confidence.ShouldBe(EstimateConfidence.Low);
        unknown.AreaAssumed.ShouldBeTrue();
    }

    [Fact]
    public void German_and_catalan_listings_are_understood()
    {
        Estimate("Renovierungsbedürftige Wohnung, Originalzustand", 1985).Level.ShouldBe(RenovationLevel.Full);
        Estimate("Pis per actualitzar amb cuina original", 1990).Level.ShouldBe(RenovationLevel.Medium);
        Estimate("Neubau, Erstbezug", null).Level.ShouldBe(RenovationLevel.ReadyToMoveIn);
    }

    [Fact]
    public void Manual_override_prevails()
    {
        var items = Estimate("a reformar", 1970, manual: new RenovationOverride(null, [new RenovationOverrideItem("kitchen", "Cocina IKEA", 6_500m), new RenovationOverrideItem("painting", "Pintura", 1_800m)], null, "Visto el sábado", DateTimeOffset.UnixEpoch));
        var level = Estimate("a reformar", 1970, manual: new RenovationOverride(RenovationLevel.Light, [], null, null, DateTimeOffset.UnixEpoch));

        items.Source.ShouldBe(EstimateSource.Manual);
        items.Cost.Probable.ShouldBe(8_300m);
        items.Cost.Min.ShouldBe(8_300m);
        level.Level.ShouldBe(RenovationLevel.Light);
        level.Source.ShouldBe(EstimateSource.Manual);
        level.Confidence.ShouldBe(EstimateConfidence.High);
    }

    [Fact]
    public void New_builds_need_no_renovation() => Estimate("Vivienda", null, newBuild: true).Cost.Probable.ShouldBe(0m);

    [Fact]
    public void Keyword_rules_detect_red_flags_and_respect_exclusions()
    {
        var flagged = Detect("Vivienda ocupada, solo inversores, con cargas. Sin cédula de habitabilidad.");
        var clean = Detect("Libre de cargas. Sin embargo, la cocina tiene un estilo rústico. Ideal inversores. IEE favorable.", year: 1960);

        flagged.Select(flag => flag.Code).ShouldBe(["occupied", "noHabitabilityCertificate", "investorsOnly", "charges"], ignoreOrder: true);
        flagged.Single(flag => flag.Code == "occupied").Severity.ShouldBe(RedFlagSeverity.Critical);
        clean.ShouldBeEmpty();
    }

    [Fact]
    public void Data_rules_cover_possession_area_rustic_land_and_old_buildings()
    {
        var cadastre = new CadastralFacts("07040A00100123", 70m, 1962, "Residencial", true, CadastralMatchQuality.Unit);

        var flags = Detect(null, PossessionStatus.NoPossession, 100m, 1962, cadastre);

        flags.Select(flag => flag.Code).ShouldBe(["noPossession", "areaMismatchHigh", "ruralLand", "buildingOver50NoIee"], ignoreOrder: true);
        Detect(null, advertised: 72m, cadastre: cadastre with { IsRustic = false, YearBuilt = 2000 }).ShouldBeEmpty();
        Detect(null, advertised: 84m, cadastre: cadastre with { IsRustic = false, YearBuilt = 2000 }).Single().Code.ShouldBe("areaMismatch");
    }

    private static IReadOnlyList<RedFlag> Detect(string? description, PossessionStatus possession = PossessionStatus.Unknown, decimal? advertised = null, int? year = null, CadastralFacts? cadastre = null) =>
        RedFlagDetector.Detect(new RedFlagInput("Piso en Palma", description, possession, advertised, year, cadastre, 2026), Flags);
}
