using HomeRadar.Application.Analysis;
using HomeRadar.Application.Ports;
using HomeRadar.Domain.Common;
using HomeRadar.Domain.Intelligence;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.Sources;
using HomeRadar.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace HomeRadar.Application.Intelligence;

public sealed record ListingInsight(
    StoredListingAnalysis? Financial,
    RenovationEstimate Renovation,
    IReadOnlyList<RedFlag> RedFlags,
    CadastralFacts? Cadastre,
    MarketSnapshot? Market,
    decimal? DeviationFromMarketPercent,
    Money? EstimatedValue,
    Money? EstimatedRenovatedValue,
    NegotiationEstimate Negotiation,
    OpportunityScore Score,
    BidRecommendation Bid,
    SourceKind SourceKind,
    DateTimeOffset ComputedAt,
    DomainError? FinancialUnavailable = null);

public sealed class ListingInsightService(IHomeRadarDbContext db, AnalysisService analysis, ComparablesService comparables, TimeProvider clock)
{
    public async Task<Result<ListingInsight>> GetAsync(Guid listingId, Guid? financingProfileId, CancellationToken cancellationToken)
    {
        var listing = await db.Listings.SingleOrDefaultAsync(candidate => candidate.Id == listingId, cancellationToken);
        if (listing is null)
        {
            return AnalysisErrors.ListingNotFound(listingId);
        }

        var profile = await analysis.ResolveProfileAsync(financingProfileId, cancellationToken);
        if (!profile.IsSuccess && (financingProfileId is not null || profile.Error!.Code != "financingProfile.noDefault"))
        {
            return Result.Failure<ListingInsight>(profile.Error!);
        }

        var insight = await RefreshAsync(listing, profile.Value, cancellationToken);
        if (insight.IsSuccess)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return insight;
    }

    public async Task<Result<ListingInsight>> BuildAsync(Listing listing, FinancingProfile? profile, CancellationToken cancellationToken)
    {
        var configuration = await analysis.ConfigurationAsync(null, cancellationToken);
        if (!configuration.IsSuccess)
        {
            return Result.Failure<ListingInsight>(configuration.Error!);
        }

        var config = configuration.Value!.Config;
        var now = clock.GetUtcNow();
        var renovation = await analysis.RenovationAsync(listing, config, cancellationToken);

        StoredListingAnalysis? financial = null;
        if (profile is not null && listing.Price is not null)
        {
            var stored = await analysis.AnalyzeAsync(listing, profile, renovation, cancellationToken);
            if (!stored.IsSuccess)
            {
                return Result.Failure<ListingInsight>(stored.Error!);
            }

            financial = stored.Value;
        }

        var financialUnavailable = financial is not null ? null : listing.Price is null ? AnalysisErrors.ListingWithoutPrice(listing.Id) : AnalysisErrors.NoDefaultProfile();

        var flags = RedFlagDetector.Detect(new RedFlagInput(listing.Title, listing.Description, listing.Possession, listing.BuiltSquareMetres, listing.YearBuilt, listing.Cadastre, now.Year), config.RedFlags);
        var market = await comparables.SnapshotAsync(listing, config.Comparables, now, cancellationToken);
        var deviation = market is not null && listing.PricePerSquareMetre is { } pricePerSquareMetre ? market.DeviationPercent(pricePerSquareMetre) : (decimal?)null;
        var area = listing.BuiltSquareMetres;
        Money? estimatedValue = market is not null && area is { } builtArea ? Money.Euros(market.MedianPricePerSquareMetre * builtArea) : null;
        Money? renovatedValue = market?.RenovatedMedianPricePerSquareMetre is { } renovatedRate && area is { } renovatedArea ? Money.Euros(renovatedRate * renovatedArea) : null;

        var relisted = await db.ListingEvents.CountAsync(listingEvent => listingEvent.ListingId == listing.Id && listingEvent.Type == ListingEventType.Relisted, cancellationToken);
        var negotiation = NegotiationEstimator.Estimate(
            new NegotiationInput(listing.Price, listing.SellerType, listing.IsNewBuild, listing.FirstSeenAt, now, listing.PriceDropCount, listing.CumulativeDropPercent, relisted, listing.Title, listing.Description, deviation, renovation.Level, flags.Any(flag => flag.Severity == RedFlagSeverity.Critical)),
            config.Negotiation);

        var sourceKind = await db.Sources.Where(source => source.Id == listing.SourceId).Select(source => source.Kind).SingleOrDefaultAsync(cancellationToken);
        var score = OpportunityScorer.Score(new ScoreContext(listing.Price, deviation, financial?.Analysis.Base.Verdict, negotiation, renovation, flags, sourceKind, config.Scoring, config.RedFlags));
        var maximum = financial?.Analysis.MaximumPrice?.Price;
        var noAffordablePrice = financial is not null && maximum is null;
        var bid = BidAdvisor.Recommend(listing.Price, negotiation, maximum, estimatedValue, config.Bidding, noAffordablePrice);

        return new ListingInsight(financial, renovation, flags, listing.Cadastre, market, deviation, estimatedValue, renovatedValue, negotiation, score, bid, sourceKind, now, financialUnavailable);
    }

    public async Task<Result<ListingInsight>> RefreshAsync(Listing listing, FinancingProfile? defaultProfile, CancellationToken cancellationToken)
    {
        var insight = await BuildAsync(listing, defaultProfile, cancellationToken);
        if (insight.IsSuccess)
        {
            var value = insight.Value!;
            listing.ApplyIntelligence(value.Renovation.Level, value.Renovation.Cost.Probable, value.Renovation.Source == EstimateSource.Manual, value.Score.Value, value.RedFlags.Select(flag => flag.Code), value.ComputedAt);
        }

        return insight;
    }
}
