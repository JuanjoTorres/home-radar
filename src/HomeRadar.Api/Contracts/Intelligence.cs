using HomeRadar.Application.Intelligence;
using HomeRadar.Domain.Intelligence;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Api.Contracts;

/// <summary>Rule-based intelligence about a listing: renovation, red flags, Catastro data, market comparison, negotiation margin, opportunity score and bidding advice. Every figure is an estimate with its explanation, never a certainty.</summary>
public sealed record ListingIntelligenceDto
{
    /// <summary>Renovation estimate (or the manual override, which always prevails).</summary>
    public required RenovationEstimateDto Renovation { get; init; }

    /// <summary>Red flags detected by keyword and cross-data rules.</summary>
    public required IReadOnlyList<RedFlagDto> RedFlags { get; init; }

    /// <summary>Catastro data matched to the listing, when any.</summary>
    public CadastreDto? Cadastre { get; init; }

    /// <summary>Comparison with similar listings in HomeRadar's own database; null when there are fewer than the configured minimum.</summary>
    public MarketDto? Market { get; init; }

    /// <summary>Estimated negotiation margin.</summary>
    public required NegotiationDto Negotiation { get; init; }

    /// <summary>Opportunity score 0–100 with its breakdown.</summary>
    public required ScoreDto Score { get; init; }

    /// <summary>Opening offer, recommended bid and walk-away price.</summary>
    public required BidDto Bid { get; init; }

    /// <summary>When this intelligence was computed (UTC).</summary>
    public required DateTimeOffset ComputedAt { get; init; }

    public static ListingIntelligenceDto From(ListingInsight insight) => new()
    {
        Renovation = RenovationEstimateDto.From(insight.Renovation),
        RedFlags = insight.RedFlags.Select(RedFlagDto.From).ToList(),
        Cadastre = insight.Cadastre is { } cadastre ? CadastreDto.From(cadastre) : null,
        Market = insight.Market is { } market ? MarketDto.From(market, insight.DeviationFromMarketPercent, insight.EstimatedValue, insight.EstimatedRenovatedValue) : null,
        Negotiation = NegotiationDto.From(insight.Negotiation),
        Score = ScoreDto.From(insight.Score),
        Bid = BidDto.From(insight.Bid),
        ComputedAt = insight.ComputedAt,
    };
}

/// <summary>Renovation estimate: total = the higher of level €/m² × built area and the sum of the level's items, plus permits and ICIO; items are an indicative breakdown and the remainder is reported as otherWorks. Costs come from the configurable Mallorca table (VAT included).</summary>
public sealed record RenovationEstimateDto
{
    /// <summary>ReadyToMoveIn, Light (paint, floors), Medium (kitchen and bathrooms) or Full (integral).</summary>
    /// <example>Medium</example>
    public required string Level { get; init; }

    /// <summary>Rules (estimated) or Manual (set with setRenovationOverride).</summary>
    public required string Source { get; init; }

    /// <summary>Low, Medium or High: how much the level classification can be trusted.</summary>
    public required string Confidence { get; init; }

    /// <summary>Lower bound of the cost, VAT included.</summary>
    public required MoneyDto Min { get; init; }

    /// <summary>Most likely cost, VAT included; used in the analysis.</summary>
    public required MoneyDto Probable { get; init; }

    /// <summary>Upper bound of the cost, VAT included.</summary>
    public required MoneyDto Max { get; init; }

    /// <summary>Built area used, in m².</summary>
    public required decimal AreaSquareMetres { get; init; }

    /// <summary>True when the listing gives no built area and the configured default was used.</summary>
    public required bool AreaAssumed { get; init; }

    /// <summary>Cost breakdown by item.</summary>
    public required IReadOnlyList<RenovationLineDto> Lines { get; init; }

    /// <summary>Why this level: keywords found, construction year, manual input.</summary>
    public required IReadOnlyList<string> Signals { get; init; }

    /// <summary>What to check on the visit for this level.</summary>
    public required IReadOnlyList<string> VisitChecklist { get; init; }

    /// <summary>Notes of the manual override.</summary>
    public string? Notes { get; init; }

    public static RenovationEstimateDto From(RenovationEstimate estimate) => new()
    {
        Level = estimate.Level.ToString(),
        Source = estimate.Source.ToString(),
        Confidence = estimate.Confidence.ToString(),
        Min = new MoneyDto(estimate.Cost.Min),
        Probable = new MoneyDto(estimate.Cost.Probable),
        Max = new MoneyDto(estimate.Cost.Max),
        AreaSquareMetres = estimate.AreaSquareMetres,
        AreaAssumed = estimate.AreaAssumed,
        Lines = estimate.Lines.Select(line => new RenovationLineDto(line.Code, line.Name, line.Quantity, line.Unit.ToString(), new MoneyDto(line.Cost.Min), new MoneyDto(line.Cost.Probable), new MoneyDto(line.Cost.Max), line.Manual)).ToList(),
        Signals = estimate.Signals,
        VisitChecklist = estimate.VisitChecklist,
        Notes = estimate.Notes,
    };
}

/// <summary>One renovation item.</summary>
/// <param name="Code">Item code, e.g. kitchen, bathroom, flooring.</param>
/// <param name="Name">Item name.</param>
/// <param name="Quantity">Quantity (m², units or 1 per dwelling).</param>
/// <param name="Unit">SquareMetre, Unit or Dwelling.</param>
/// <param name="Min">Lower bound.</param>
/// <param name="Probable">Most likely cost.</param>
/// <param name="Max">Upper bound.</param>
/// <param name="Manual">True when set manually.</param>
public sealed record RenovationLineDto(string Code, string Name, decimal Quantity, string Unit, MoneyDto Min, MoneyDto Probable, MoneyDto Max, bool Manual);

/// <summary>Red flag.</summary>
/// <param name="Code">Stable code, e.g. possessionIssue, noHabitabilityCertificate, areaMismatch, rusticLand.</param>
/// <param name="Severity">Low, Medium, High or Critical.</param>
/// <param name="Description">Why it matters.</param>
/// <param name="Evidence">What triggered it (phrase found or data compared).</param>
public sealed record RedFlagDto(string Code, string Severity, string Description, string Evidence)
{
    public static RedFlagDto From(RedFlag flag) => new(flag.Code, flag.Severity.ToString(), flag.Description, flag.Evidence);
}

/// <summary>Catastro data matched to the listing (free OVC services: no owners, no cadastral value).</summary>
/// <param name="Reference">20-character unit reference, or 14-character parcel reference when no unit could be matched.</param>
/// <param name="BuiltSquareMetres">Built area of the unit in m², including its share of common elements.</param>
/// <param name="YearBuilt">Construction year.</param>
/// <param name="Use">Main use, e.g. Residencial.</param>
/// <param name="Rustic">True when the parcel is rustic land.</param>
/// <param name="Match">Unit (one unit identified), Parcel (only the building), Manual (set with setCadastralData) or None.</param>
public sealed record CadastreDto(string? Reference, decimal? BuiltSquareMetres, int? YearBuilt, string? Use, bool? Rustic, string Match)
{
    public static CadastreDto From(CadastralFacts facts) => new(facts.Reference, facts.BuiltSquareMetres, facts.YearBuilt, facts.Use, facts.IsRustic, facts.Match.ToString());
}

/// <summary>Market comparison with similar active listings (same property family, built area within the tolerance, seen in the last year): first within the radius, then the municipality, then the island.</summary>
public sealed record MarketDto
{
    /// <summary>Radius, Municipality or Island.</summary>
    public required string Scope { get; init; }

    /// <summary>Number of comparable listings.</summary>
    public required int Count { get; init; }

    /// <summary>Median asking price per built m² (EUR/m²).</summary>
    public required decimal MedianPricePerSquareMetre { get; init; }

    /// <summary>25th percentile (EUR/m²).</summary>
    public required decimal P25PricePerSquareMetre { get; init; }

    /// <summary>75th percentile (EUR/m²).</summary>
    public required decimal P75PricePerSquareMetre { get; init; }

    /// <summary>Median of the comparables classified ReadyToMoveIn (EUR/m²), when there are enough.</summary>
    public decimal? RenovatedMedianPricePerSquareMetre { get; init; }

    /// <summary>Low, Medium or High.</summary>
    public required string Confidence { get; init; }

    /// <summary>Listing €/m² versus the median, in % (positive = more expensive).</summary>
    public decimal? DeviationPercent { get; init; }

    /// <summary>Median €/m² × built area: an asking-price based estimate, not an appraisal.</summary>
    public MoneyDto? EstimatedValue { get; init; }

    /// <summary>Renovated median €/m² × built area: value of an equivalent renovated home.</summary>
    public MoneyDto? EstimatedRenovatedValue { get; init; }

    public static MarketDto From(MarketSnapshot market, decimal? deviation, Money? value, Money? renovatedValue) => new()
    {
        Scope = market.Scope.ToString(),
        Count = market.Count,
        MedianPricePerSquareMetre = market.MedianPricePerSquareMetre,
        P25PricePerSquareMetre = market.P25PricePerSquareMetre,
        P75PricePerSquareMetre = market.P75PricePerSquareMetre,
        RenovatedMedianPricePerSquareMetre = market.RenovatedMedianPricePerSquareMetre,
        Confidence = market.Confidence.ToString(),
        DeviationPercent = deviation,
        EstimatedValue = MoneyDto.From(value),
        EstimatedRenovatedValue = MoneyDto.From(renovatedValue),
    };
}

/// <summary>Negotiation margin as a range: base gap for the seller type + price drops + time on the market + relisting + urgency wording + part of the premium over comparables, capped by configuration.</summary>
public sealed record NegotiationDto
{
    /// <summary>False when there is no margin to work with (for example a Critical red flag): do not bid until it is cleared.</summary>
    public required bool Applicable { get; init; }

    /// <summary>Lower bound of the discount, in % of the asking price.</summary>
    public required decimal MinPercent { get; init; }

    /// <summary>Most likely discount, in %.</summary>
    public required decimal ProbablePercent { get; init; }

    /// <summary>Upper bound, in %.</summary>
    public required decimal MaxPercent { get; init; }

    /// <summary>Lower bound in EUR.</summary>
    public MoneyDto? MinAmount { get; init; }

    /// <summary>Most likely discount in EUR.</summary>
    public MoneyDto? ProbableAmount { get; init; }

    /// <summary>Upper bound in EUR.</summary>
    public MoneyDto? MaxAmount { get; init; }

    /// <summary>Low, Medium or High.</summary>
    public required string Confidence { get; init; }

    /// <summary>Contribution of each factor, in percentage points.</summary>
    public required IReadOnlyList<NegotiationFactorDto> Factors { get; init; }

    public static NegotiationDto From(NegotiationEstimate estimate) => new()
    {
        Applicable = estimate.Applicable,
        MinPercent = estimate.MinPercent,
        ProbablePercent = estimate.ProbablePercent,
        MaxPercent = estimate.MaxPercent,
        MinAmount = MoneyDto.From(estimate.MinAmount),
        ProbableAmount = MoneyDto.From(estimate.ProbableAmount),
        MaxAmount = MoneyDto.From(estimate.MaxAmount),
        Confidence = estimate.Confidence.ToString(),
        Factors = estimate.Factors.Select(factor => new NegotiationFactorDto(factor.Code, factor.ContributionPercent, factor.Explanation)).ToList(),
    };
}

/// <summary>Factor of the negotiation margin.</summary>
/// <param name="Code">sellerType, priceDrops, largeCumulativeDrop, daysOnMarket, freshListing, relisted, urgency, needsRenovation, priceVsMarket, season or criticalFlag.</param>
/// <param name="ContributionPercent">Percentage points it adds.</param>
/// <param name="Explanation">Why.</param>
public sealed record NegotiationFactorDto(string Code, decimal ContributionPercent, string Explanation);

/// <summary>Opportunity score: weighted average of the available components (weights in the business configuration), capped at 30 when there is a Critical red flag.</summary>
/// <param name="Value">Score 0–100.</param>
/// <param name="Components">Each component with its points (0–100), weight and explanation; components without data have null points and do not count.</param>
public sealed record ScoreDto(int Value, IReadOnlyList<ScoreComponentDto> Components)
{
    public static ScoreDto From(OpportunityScore score) => new(score.Value, score.Components.Select(component => new ScoreComponentDto(component.Code, component.Points, component.Weight, component.Explanation)).ToList());
}

/// <summary>Score component.</summary>
/// <param name="Code">priceVsMarket, affordability, negotiation, renovation, risk or competition.</param>
/// <param name="Points">0–100, or null when not available.</param>
/// <param name="Weight">Configured weight.</param>
/// <param name="Explanation">What drives it.</param>
public sealed record ScoreComponentDto(string Code, decimal? Points, decimal Weight, string Explanation);

/// <summary>Bidding advice, rounded down to 500 EUR. Walk-away price = the lower of your maximum affordable price (renovation included) and the estimated value plus the configured tolerance.</summary>
/// <param name="OpeningOffer">Asking price minus the maximum margin, never above the recommended bid.</param>
/// <param name="RecommendedBid">Asking price minus the probable margin, capped by the walk-away price.</param>
/// <param name="WalkAwayPrice">Do not pay more than this.</param>
/// <param name="Rationale">How each figure was derived.</param>
public sealed record BidDto(MoneyDto? OpeningOffer, MoneyDto? RecommendedBid, MoneyDto? WalkAwayPrice, IReadOnlyList<string> Rationale)
{
    public static BidDto From(BidRecommendation bid) => new(MoneyDto.From(bid.OpeningOffer), MoneyDto.From(bid.RecommendedBid), MoneyDto.From(bid.WalkAwayPrice), bid.Rationale);
}

/// <summary>Manual renovation override; it prevails over the rule-based estimate. Give items (their sum is the cost), a total budget, or just a level (the cost is estimated for that level).</summary>
public sealed record RenovationOverrideRequestDto
{
    /// <summary>ReadyToMoveIn, Light, Medium or Full.</summary>
    public string? Level { get; init; }

    /// <summary>Specific items with their amount.</summary>
    public IReadOnlyList<RenovationOverrideItemDto> Items { get; init; } = [];

    /// <summary>Total renovation budget.</summary>
    public MoneyDto? TotalBudget { get; init; }

    /// <summary>Free notes, e.g. what you saw on the visit.</summary>
    public string? Notes { get; init; }
}

/// <summary>Manual renovation item.</summary>
/// <param name="Code">Item code, e.g. kitchen.</param>
/// <param name="Description">What it covers.</param>
/// <param name="Amount">Amount, VAT included.</param>
public sealed record RenovationOverrideItemDto(string Code, string Description, MoneyDto Amount);

/// <summary>Renovation estimate for a listing or for manual parameters.</summary>
public sealed record RenovationEstimateRequestDto
{
    /// <summary>Listing to estimate; the remaining fields override its data when given.</summary>
    public Guid? ListingId { get; init; }

    /// <summary>Built area in m².</summary>
    public decimal? BuiltSquareMetres { get; init; }

    /// <summary>Bathrooms.</summary>
    public int? Bathrooms { get; init; }

    /// <summary>Bedrooms.</summary>
    public int? Rooms { get; init; }

    /// <summary>Construction year.</summary>
    public int? YearBuilt { get; init; }

    /// <summary>Property type.</summary>
    public string? PropertyType { get; init; }

    /// <summary>New build.</summary>
    public bool? IsNewBuild { get; init; }

    /// <summary>Description to scan for condition keywords.</summary>
    public string? Description { get; init; }

    /// <summary>Force a level instead of classifying it.</summary>
    public string? Level { get; init; }
}

/// <summary>Cadastral data you checked yourself; it prevails over the automatic match.</summary>
public sealed record CadastralDataRequestDto
{
    /// <summary>20-character cadastral reference of the unit.</summary>
    /// <example>0763604DD7806C0001UY</example>
    public string? CadastralReference { get; init; }

    /// <summary>Built area in m² according to the Catastro.</summary>
    public decimal? BuiltSquareMetres { get; init; }

    /// <summary>Construction year according to the Catastro.</summary>
    public int? YearBuilt { get; init; }

    /// <summary>Cadastral reference value (valor de referencia) you consulted in the Sede del Catastro; the tax base becomes the higher of price and this value.</summary>
    public MoneyDto? ReferenceValue { get; init; }

    /// <summary>Date the reference value refers to.</summary>
    public DateOnly? ReferenceValueDate { get; init; }
}

/// <summary>Cadastral data stored for a listing.</summary>
/// <param name="Cadastre">Catastro facts used by the analysis.</param>
/// <param name="ReferenceValue">Reference value entered manually.</param>
/// <param name="ReferenceValueDate">Date of that value.</param>
public sealed record CadastralDataDto(CadastreDto? Cadastre, MoneyDto? ReferenceValue, DateOnly? ReferenceValueDate);
