using CsCheck;
using HomeRadar.Application.Tests.Finance;
using HomeRadar.Domain.Finance;
using HomeRadar.Domain.Intelligence;
using HomeRadar.Domain.Sources;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Application.Tests.Intelligence;

public sealed class MarketAndScoreTests
{
    private static readonly BusinessConfig Config = FinanceFixture.Config;
    private static readonly DateTimeOffset June = new(2026, 6, 10, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset November = new(2026, 11, 15, 9, 0, 0, TimeSpan.Zero);

    private static NegotiationEstimate Negotiate(SellerType seller, int days, int drops, string? description, DateTimeOffset now, decimal? deviation = null, bool critical = false, bool newBuild = false, RenovationLevel? level = null) =>
        NegotiationEstimator.Estimate(new NegotiationInput(Money.Euros(250_000m), seller, newBuild, now.AddDays(-days), now, drops, drops * 4m, 0, "Piso", description, deviation, level, critical), Config.Negotiation);

    [Fact]
    public void Research_example_gives_thirteen_and_a_half_percent_in_low_season()
    {
        var estimate = Negotiate(SellerType.Private, 120, 2, "Precio negociable", November);

        estimate.ProbablePercent.ShouldBe(13.5m);
        estimate.MinPercent.ShouldBe(10.5m);
        estimate.MaxPercent.ShouldBe(15m);
        estimate.ProbableAmount!.Value.Amount.ShouldBe(33_750m);
        estimate.Factors.Select(factor => factor.Code).ShouldBe(["sellerType", "priceDrops", "daysOnMarket", "urgency", "season"]);
    }

    [Fact]
    public void Fresh_servicer_listing_starts_lower_and_new_builds_barely_move()
    {
        Negotiate(SellerType.Servicer, 5, 0, null, June).ProbablePercent.ShouldBe(6m);
        Negotiate(SellerType.Agency, 90, 0, null, June, newBuild: true).ProbablePercent.ShouldBe(3m);
        Negotiate(SellerType.Agency, 30, 0, null, June, deviation: 35m, level: RenovationLevel.Full).ProbablePercent.ShouldBe(9.5m);
    }

    [Fact]
    public void Critical_flags_disable_the_margin()
    {
        var estimate = Negotiate(SellerType.Servicer, 200, 3, "urge venta", November, critical: true);

        estimate.Applicable.ShouldBeFalse();
        estimate.ProbablePercent.ShouldBe(0m);
    }

    [Fact]
    public void Margin_stays_within_bounds_for_any_history() =>
        Gen.Select(Gen.Int[0, 1_000], Gen.Int[0, 10], Gen.Decimal[-50m, 100m], Gen.Bool)
            .Sample((days, drops, deviation, urgent) =>
            {
                var estimate = Negotiate(SellerType.Agency, days, drops, urgent ? "se escuchan ofertas" : null, November, Math.Round(deviation, 1));
                estimate.MinPercent.ShouldBeInRange(0m, estimate.ProbablePercent);
                estimate.MaxPercent.ShouldBeInRange(estimate.ProbablePercent, Config.Negotiation.MaxMarginPercent);
            },
            iter: 200);

    [Fact]
    public void Market_statistics_use_interpolated_percentiles()
    {
        var points = new[] { 2_000m, 2_500m, 3_000m, 3_500m, 4_000m, 5_000m }.Select((rate, index) => new ComparablePoint(Guid.NewGuid(), rate, index < 3 ? RenovationLevel.ReadyToMoveIn : RenovationLevel.Medium)).ToList();

        var market = MarketStatistics.Build(points, ComparableScope.Municipality, Config.Comparables)!;

        market.MedianPricePerSquareMetre.ShouldBe(3_250m);
        market.P25PricePerSquareMetre.ShouldBe(2_625m);
        market.P75PricePerSquareMetre.ShouldBe(3_875m);
        market.RenovatedMedianPricePerSquareMetre.ShouldBe(2_500m);
        market.DeviationPercent(3_900m).ShouldBe(20m);
        MarketStatistics.Build(points.Take(4).ToList(), ComparableScope.Radius, Config.Comparables).ShouldBeNull();
    }

    [Fact]
    public void Score_weights_available_components_and_caps_critical_risks()
    {
        var negotiation = Negotiate(SellerType.Servicer, 120, 1, null, June);
        var renovation = new RenovationEstimate(RenovationLevel.Light, EstimateSource.Rules, EstimateConfidence.Medium, new CostRange(10_000m, 20_000m, 30_000m), 80m, false, [], [], [], null);
        var context = new ScoreContext(Money.Euros(200_000m), -10m, AffordabilityVerdict.Affordable, negotiation, renovation, [], SourceKind.Servicer, Config.Scoring, Config.RedFlags);

        var score = OpportunityScorer.Score(context);
        var critical = OpportunityScorer.Score(context with { Flags = [new RedFlag("occupied", RedFlagSeverity.Critical, "", "")] });
        var unknownMarket = OpportunityScorer.Score(context with { DeviationFromMarketPercent = null });

        score.Components.Single(component => component.Code == "priceVsMarket").Points.ShouldBe(70m);
        score.Components.Single(component => component.Code == "renovation").Points.ShouldBe(75m);
        score.Components.Single(component => component.Code == "negotiation").Points.ShouldBe(76.7m);
        score.Value.ShouldBe(85);
        critical.Value.ShouldBeLessThanOrEqualTo(30);
        unknownMarket.Components.Single(component => component.Code == "priceVsMarket").Points.ShouldBeNull();
        unknownMarket.Value.ShouldBe(90);
    }

    [Fact]
    public void Bid_is_capped_by_affordability_and_value()
    {
        var negotiation = Negotiate(SellerType.Agency, 90, 1, null, June);

        var limitedByValue = BidAdvisor.Recommend(Money.Euros(300_000m), negotiation, Money.Euros(320_000m), Money.Euros(260_000m), Config.Bidding);
        var limitedByMargin = BidAdvisor.Recommend(Money.Euros(300_000m), negotiation, Money.Euros(320_000m), Money.Euros(330_000m), Config.Bidding);

        negotiation.ProbablePercent.ShouldBe(8.5m);
        limitedByValue.WalkAwayPrice!.Value.Amount.ShouldBe(273_000m);
        limitedByValue.RecommendedBid!.Value.Amount.ShouldBe(273_000m);
        limitedByMargin.RecommendedBid!.Value.Amount.ShouldBe(274_500m);
        limitedByMargin.OpeningOffer!.Value.Amount.ShouldBe(262_500m);
        limitedByMargin.WalkAwayPrice!.Value.Amount.ShouldBe(320_000m);
        var unaffordable = BidAdvisor.Recommend(Money.Euros(300_000m), negotiation, null, Money.Euros(330_000m), Config.Bidding, affordableAtNoPrice: true);
        unaffordable.RecommendedBid.ShouldBeNull();
        unaffordable.WalkAwayPrice.ShouldBeNull();
    }
}
