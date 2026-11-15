using System.Globalization;
using HomeRadar.Domain.Finance;
using HomeRadar.Domain.Sources;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Domain.Intelligence;

public enum ComparableScope
{
    Radius = 0,
    Municipality = 1,
    Island = 2,
}

public sealed record ComparablePoint(Guid ListingId, decimal PricePerSquareMetre, RenovationLevel? Level);

public sealed record MarketSnapshot(
    ComparableScope Scope,
    int Count,
    decimal MedianPricePerSquareMetre,
    decimal P25PricePerSquareMetre,
    decimal P75PricePerSquareMetre,
    decimal? RenovatedMedianPricePerSquareMetre,
    int RenovatedCount,
    EstimateConfidence Confidence)
{
    public decimal DeviationPercent(decimal pricePerSquareMetre) =>
        Math.Round((pricePerSquareMetre - MedianPricePerSquareMetre) / MedianPricePerSquareMetre * 100m, 2, MidpointRounding.ToEven);
}

public static class MarketStatistics
{
    public static MarketSnapshot? Build(IReadOnlyList<ComparablePoint> points, ComparableScope scope, ComparablesConfig config)
    {
        if (points.Count < config.MinimumCount)
        {
            return null;
        }

        var sorted = points.Select(point => point.PricePerSquareMetre).Order().ToList();
        var renovated = points.Where(point => point.Level is RenovationLevel.ReadyToMoveIn).Select(point => point.PricePerSquareMetre).Order().ToList();
        var confidence = scope switch
        {
            ComparableScope.Radius when points.Count >= config.MinimumCount * 3 => EstimateConfidence.High,
            ComparableScope.Island => EstimateConfidence.Low,
            _ => EstimateConfidence.Medium,
        };
        return new MarketSnapshot(
            scope,
            points.Count,
            Percentile(sorted, 0.5m),
            Percentile(sorted, 0.25m),
            Percentile(sorted, 0.75m),
            renovated.Count >= Math.Max(3, config.MinimumCount / 2) ? Percentile(renovated, 0.5m) : null,
            renovated.Count,
            confidence);
    }

    public static decimal Percentile(IReadOnlyList<decimal> sorted, decimal fraction)
    {
        if (sorted.Count == 1)
        {
            return sorted[0];
        }

        var position = fraction * (sorted.Count - 1);
        var lower = (int)Math.Floor(position);
        var upper = Math.Min(lower + 1, sorted.Count - 1);
        return Math.Round(sorted[lower] + ((sorted[upper] - sorted[lower]) * (position - lower)), 2, MidpointRounding.ToEven);
    }
}

public sealed record ScoreContext(
    Money? Price,
    decimal? DeviationFromMarketPercent,
    AffordabilityVerdict? Verdict,
    NegotiationEstimate? Negotiation,
    RenovationEstimate? Renovation,
    IReadOnlyList<RedFlag> Flags,
    SourceKind SourceKind,
    ScoringConfig Scoring,
    RedFlagConfig RedFlags);

public sealed record ScoreComponentResult(string Code, decimal? Points, decimal Weight, string Explanation);

public sealed record OpportunityScore(int Value, IReadOnlyList<ScoreComponentResult> Components);

public interface IScoreComponent
{
    string Code { get; }

    decimal DefaultWeight { get; }

    (decimal? Points, string Explanation) Evaluate(ScoreContext context);
}

public sealed class PriceVsMarketComponent : IScoreComponent
{
    public string Code => "priceVsMarket";

    public decimal DefaultWeight => 25m;

    public (decimal? Points, string Explanation) Evaluate(ScoreContext context) => context.DeviationFromMarketPercent is { } deviation
        ? (Clamp(50m - (deviation * 50m / context.Scoring.FullScoreDiscountPercent)), string.Create(CultureInfo.InvariantCulture, $"€/m² {deviation:+0.#;-0.#;0} % versus comparable listings"))
        : (null, "Not enough comparable listings");

    internal static decimal Clamp(decimal value) => Math.Round(Math.Clamp(value, 0m, 100m), 1, MidpointRounding.ToEven);
}

public sealed class AffordabilityComponent : IScoreComponent
{
    public string Code => "affordability";

    public decimal DefaultWeight => 20m;

    public (decimal? Points, string Explanation) Evaluate(ScoreContext context) => context.Verdict switch
    {
        AffordabilityVerdict.Affordable => (100m, "Affordable with the profile"),
        AffordabilityVerdict.Borderline => (50m, "Borderline with the profile"),
        AffordabilityVerdict.NotAffordable => (0m, "Not affordable with the profile"),
        _ => (null, "No financing profile or no price"),
    };
}

public sealed class NegotiationComponent : IScoreComponent
{
    public string Code => "negotiation";

    public decimal DefaultWeight => 10m;

    public (decimal? Points, string Explanation) Evaluate(ScoreContext context) => context.Negotiation is { Applicable: true } negotiation
        ? (PriceVsMarketComponent.Clamp(negotiation.ProbablePercent / context.Scoring.FullMarginPercent * 100m), string.Create(CultureInfo.InvariantCulture, $"Probable negotiation margin {negotiation.ProbablePercent:0.#} %"))
        : (null, "No negotiation estimate");
}

public sealed class RenovationComponent : IScoreComponent
{
    public string Code => "renovation";

    public decimal DefaultWeight => 15m;

    public (decimal? Points, string Explanation) Evaluate(ScoreContext context)
    {
        if (context.Renovation is not { } renovation || context.Price is not { Amount: > 0m } price)
        {
            return (null, "No renovation estimate or no price");
        }

        var share = renovation.Cost.Probable / price.Amount * 100m;
        return (PriceVsMarketComponent.Clamp(100m - (share * 100m / context.Scoring.ZeroScoreRenovationSharePercent)), string.Create(CultureInfo.InvariantCulture, $"{renovation.Level} renovation ≈ {share:0.#} % of the price"));
    }
}

public sealed class RiskComponent : IScoreComponent
{
    public string Code => "risk";

    public decimal DefaultWeight => 20m;

    public (decimal? Points, string Explanation) Evaluate(ScoreContext context)
    {
        var penalty = RedFlagDetector.RiskPenalty(context.Flags, context.RedFlags);
        return (PriceVsMarketComponent.Clamp(100m - penalty), context.Flags.Count == 0 ? "No red flags" : $"Red flags: {string.Join(", ", context.Flags.Select(flag => flag.Code))}");
    }
}

public sealed class CompetitionComponent : IScoreComponent
{
    public string Code => "competition";

    public decimal DefaultWeight => 10m;

    public (decimal? Points, string Explanation) Evaluate(ScoreContext context) =>
        (context.Scoring.CompetitionBySourceKind.TryGetValue(context.SourceKind, out var points) ? points : 50m, $"Source kind {context.SourceKind}: the less visible the source, the fewer competing buyers");
}

public static class OpportunityScorer
{
    public static IReadOnlyList<IScoreComponent> Components { get; } =
    [
        new PriceVsMarketComponent(),
        new AffordabilityComponent(),
        new NegotiationComponent(),
        new RenovationComponent(),
        new RiskComponent(),
        new CompetitionComponent(),
    ];

    public static OpportunityScore Score(ScoreContext context)
    {
        var results = Components.Select(component =>
        {
            var (points, explanation) = component.Evaluate(context);
            var weight = context.Scoring.Weights.TryGetValue(component.Code, out var configured) ? configured : component.DefaultWeight;
            return new ScoreComponentResult(component.Code, points, weight, explanation);
        }).ToList();

        var available = results.Where(result => result.Points is not null && result.Weight > 0m).ToList();
        var totalWeight = available.Sum(result => result.Weight);
        var value = totalWeight == 0m ? 0m : available.Sum(result => result.Points!.Value * result.Weight) / totalWeight;
        if (context.Flags.Any(flag => flag.Severity == RedFlagSeverity.Critical))
        {
            value = Math.Min(value, 30m);
        }

        return new OpportunityScore((int)Math.Round(value, MidpointRounding.AwayFromZero), results);
    }
}

public sealed record BidRecommendation(Money? OpeningOffer, Money? RecommendedBid, Money? WalkAwayPrice, IReadOnlyList<string> Rationale);

public static class BidAdvisor
{
    public static BidRecommendation Recommend(Money? price, NegotiationEstimate? negotiation, Money? maximumAffordablePrice, Money? estimatedValue, BiddingConfig config, bool affordableAtNoPrice = false)
    {
        if (affordableAtNoPrice)
        {
            return new BidRecommendation(null, null, null, ["No price is affordable with your profile once renovation, taxes and costs are added: lower the renovation with a manual estimate or review the financing"]);
        }

        if (price is not { } asking || negotiation is not { Applicable: true })
        {
            return new BidRecommendation(null, null, maximumAffordablePrice, ["No price or negotiation estimate"]);
        }

        var rationale = new List<string>();
        var target = asking * (1m - (negotiation.ProbablePercent / 100m));
        var opening = asking * (1m - (negotiation.MaxPercent / 100m));
        rationale.Add(string.Create(CultureInfo.InvariantCulture, $"Asking {asking.Amount:N0} EUR, probable margin {negotiation.ProbablePercent:0.#} %, maximum {negotiation.MaxPercent:0.#} %"));

        var limits = new List<Money>();
        if (maximumAffordablePrice is { } affordable)
        {
            limits.Add(affordable);
            rationale.Add(string.Create(CultureInfo.InvariantCulture, $"Your maximum affordable price, renovation included: {affordable.Amount:N0} EUR"));
        }

        if (estimatedValue is { } value)
        {
            var ceiling = value * (1m + (config.OverpayTolerancePercent / 100m));
            limits.Add(ceiling);
            rationale.Add(string.Create(CultureInfo.InvariantCulture, $"Estimated market value {value.Amount:N0} EUR; paying more than {config.OverpayTolerancePercent:0.#} % above it is not advised"));
        }

        Money? walkAway = limits.Count > 0 ? limits.Min() : null;
        if (walkAway is { } cap && cap < opening)
        {
            rationale.Add("Your walk-away price is below even the most aggressive opening offer: closing within your limits is unlikely");
        }

        var recommended = walkAway is { } limit && limit < target ? limit : target;
        if (opening > recommended)
        {
            opening = recommended;
        }

        return new BidRecommendation(Round(opening), Round(recommended), walkAway is { } w ? Round(w) : null, rationale);
    }

    private static Money Round(Money amount) => Money.Euros(Math.Round(amount.Amount / 500m, MidpointRounding.ToZero) * 500m);
}
