using System.Globalization;
using HomeRadar.Domain.Finance;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Domain.Intelligence;

public sealed record NegotiationInput(
    Money? Price,
    SellerType SellerType,
    bool IsNewBuild,
    DateTimeOffset FirstSeenAt,
    DateTimeOffset Now,
    int PriceDropCount,
    decimal CumulativeDropPercent,
    int RelistedCount,
    string? Title,
    string? Description,
    decimal? DeviationFromMarketPercent,
    RenovationLevel? RenovationLevel,
    bool HasCriticalFlag);

public sealed record NegotiationFactor(string Code, decimal ContributionPercent, string Explanation);

public sealed record NegotiationEstimate(
    bool Applicable,
    decimal MinPercent,
    decimal ProbablePercent,
    decimal MaxPercent,
    Money? MinAmount,
    Money? ProbableAmount,
    Money? MaxAmount,
    EstimateConfidence Confidence,
    IReadOnlyList<NegotiationFactor> Factors);

public static class NegotiationEstimator
{
    private const int DaysPerMonth = 30;

    public static NegotiationEstimate Estimate(NegotiationInput input, NegotiationConfig config)
    {
        if (input.HasCriticalFlag)
        {
            return new NegotiationEstimate(false, 0m, 0m, 0m, null, null, null, EstimateConfidence.Low, [new NegotiationFactor("criticalFlag", 0m, "A critical red flag makes the asking price not comparable: its discount is a risk premium, not a negotiation margin")]);
        }

        var factors = new List<NegotiationFactor>();
        if (input.IsNewBuild && config.NewBuildBaseMarginPercent is { } newBuild)
        {
            factors.Add(new NegotiationFactor("sellerType", newBuild, "New build: developers rarely negotiate"));
        }
        else
        {
            var basePercent = config.BaseMarginPercent.TryGetValue(input.SellerType, out var seller) ? seller : 0m;
            factors.Add(new NegotiationFactor("sellerType", basePercent, $"Typical gap between asking and closing price for {input.SellerType} sellers"));
        }

        var drops = Math.Min(input.PriceDropCount, config.MaxCountedPriceDrops);
        if (drops > 0)
        {
            factors.Add(new NegotiationFactor("priceDrops", drops * config.PerPriceDropPercent, string.Create(CultureInfo.InvariantCulture, $"{input.PriceDropCount} price drop(s) recorded: the seller is already moving")));
        }

        if (input.CumulativeDropPercent >= config.LargeCumulativeDropThresholdPercent && config.LargeCumulativeDropPercent != 0m)
        {
            factors.Add(new NegotiationFactor("largeCumulativeDrop", config.LargeCumulativeDropPercent, string.Create(CultureInfo.InvariantCulture, $"Already {input.CumulativeDropPercent:0.#} % below the first price seen")));
        }

        var days = Math.Max(0, (int)(input.Now - input.FirstSeenAt).TotalDays);
        if (days > config.DaysOnMarketGraceDays)
        {
            var months = (days - config.DaysOnMarketGraceDays) / DaysPerMonth;
            var contribution = Math.Min(months * config.PerThirtyDaysOnMarketPercent, config.MaxDaysOnMarketPercent);
            if (contribution > 0m)
            {
                factors.Add(new NegotiationFactor("daysOnMarket", contribution, string.Create(CultureInfo.InvariantCulture, $"{days} days on the market, above the usual {config.DaysOnMarketGraceDays}")));
            }
        }
        else if (days < config.FreshListingDays && config.FreshListingPercent != 0m)
        {
            factors.Add(new NegotiationFactor("freshListing", config.FreshListingPercent, string.Create(CultureInfo.InvariantCulture, $"Published {days} day(s) ago in a tight market")));
        }

        if (input.RelistedCount > 0 && config.RelistedPercent != 0m)
        {
            factors.Add(new NegotiationFactor("relisted", config.RelistedPercent, $"Withdrawn and published again {input.RelistedCount} time(s)"));
        }

        var words = TextKeys.Words($"{input.Title} {input.Description}");
        var urgency = config.UrgencyKeywords.FirstOrDefault(keyword => TextKeys.ContainsPhrase(words, keyword));
        if (urgency is not null)
        {
            factors.Add(new NegotiationFactor("urgency", config.UrgencyPercent, $"Text signals urgency or openness to offers: \"{urgency}\""));
        }

        if (input.RenovationLevel is RenovationLevel.Medium or RenovationLevel.Full && config.NeedsRenovationPercent != 0m)
        {
            factors.Add(new NegotiationFactor("needsRenovation", config.NeedsRenovationPercent, $"Needs a {input.RenovationLevel} renovation"));
        }

        if (input.DeviationFromMarketPercent is { } deviation && deviation > config.OverpricedThresholdPercent)
        {
            factors.Add(new NegotiationFactor("priceVsMarket", config.OverpricedPercent, string.Create(CultureInfo.InvariantCulture, $"€/m² {deviation:0.#} % above comparable listings")));
        }

        if (config.SeasonalMonths.Contains(input.Now.Month) && config.SeasonalPercent != 0m)
        {
            factors.Add(new NegotiationFactor("season", config.SeasonalPercent, "Low season for sales"));
        }

        var probable = Math.Clamp(factors.Sum(factor => factor.ContributionPercent), 0m, config.MaxMarginPercent);
        var minimum = Math.Max(0m, probable - config.RangeBelowPercent);
        var maximum = Math.Min(config.MaxMarginPercent, probable + config.RangeAbovePercent);
        var confidence = input.DeviationFromMarketPercent is not null && days >= 2 * DaysPerMonth
            ? EstimateConfidence.Medium
            : EstimateConfidence.Low;

        return new NegotiationEstimate(true, minimum, probable, maximum, Amount(input.Price, minimum), Amount(input.Price, probable), Amount(input.Price, maximum), confidence, factors);
    }

    private static Money? Amount(Money? price, decimal percent) => price is { } value ? value * (percent / 100m) : null;
}
