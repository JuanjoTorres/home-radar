using HomeRadar.Domain.Listings;

namespace HomeRadar.Application.Ports;

/// <summary>
/// Extension point of the ingestion pipeline. Enrichers add derived data to a listing (rules today, LLM later)
/// and must label every value they produce with its <see cref="EnrichmentOrigin"/>.
/// </summary>
public interface IListingEnricher
{
    /// <summary>Runs after change detection for new or changed listings only.</summary>
    /// <param name="listing">The listing being ingested; already reflects the latest observation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Enrichment facts to persist, possibly empty.</returns>
    Task<IReadOnlyList<EnrichmentFact>> EnrichAsync(Listing listing, CancellationToken cancellationToken);
}

public sealed record EnrichmentFact(string Key, string Value, EnrichmentOrigin Origin, decimal Confidence);

public enum EnrichmentOrigin
{
    Rules = 0,
    Llm = 1,
    Manual = 2,
}
