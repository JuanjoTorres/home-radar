using HomeRadar.Domain.Common;

namespace HomeRadar.Domain.Finance;

public sealed class ListingAnalysis : Entity
{
    private ListingAnalysis()
    {
    }

    public ListingAnalysis(
        Guid listingId,
        Guid financingProfileId,
        string inputHash,
        Guid businessConfigVersionId,
        IReadOnlyList<Guid> taxRuleSetIds,
        AffordabilityVerdict verdict,
        decimal priceAmount,
        decimal? maximumAffordablePrice,
        string inputJson,
        string resultJson,
        DateTimeOffset calculatedAt)
    {
        ListingId = listingId;
        FinancingProfileId = financingProfileId;
        InputHash = inputHash;
        BusinessConfigVersionId = businessConfigVersionId;
        TaxRuleSetIds = taxRuleSetIds.ToList();
        Verdict = verdict;
        PriceAmount = priceAmount;
        MaximumAffordablePrice = maximumAffordablePrice;
        InputJson = inputJson;
        ResultJson = resultJson;
        CalculatedAt = calculatedAt;
    }

    public Guid ListingId { get; private init; }

    public Guid FinancingProfileId { get; private init; }

    public string InputHash { get; private init; } = string.Empty;

    public Guid BusinessConfigVersionId { get; private init; }

    public IReadOnlyList<Guid> TaxRuleSetIds { get; private init; } = [];

    public AffordabilityVerdict Verdict { get; private init; }

    public decimal PriceAmount { get; private init; }

    public decimal? MaximumAffordablePrice { get; private init; }

    public string InputJson { get; private init; } = "{}";

    public string ResultJson { get; private init; } = "{}";

    public DateTimeOffset CalculatedAt { get; private init; }
}
