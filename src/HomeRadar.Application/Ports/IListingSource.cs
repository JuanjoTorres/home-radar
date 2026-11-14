using HomeRadar.Domain.Listings;
using HomeRadar.Domain.Sources;

namespace HomeRadar.Application.Ports;

/// <summary>
/// Adapter for one external source (a servicer portal, a CRM family, a feed format or a mailbox).
/// Implementations translate the external format into <see cref="ListingObservation"/>s and must not persist anything.
/// </summary>
public interface IListingSource
{
    /// <summary>Key used to resolve the adapter for a <see cref="Source"/> (matches <see cref="Source.AdapterKey"/>).</summary>
    string AdapterKey { get; }

    /// <summary>
    /// Streams every listing currently published by the source.
    /// </summary>
    /// <param name="context">The source definition, its JSON configuration and whether a full scan is requested.</param>
    /// <param name="cancellationToken">Cancels the fetch between pages.</param>
    /// <returns>Observations together with the raw document they were extracted from, in source order.</returns>
    /// <exception cref="SourceSchemaChangedException">When the page structure no longer matches the adapter's expectations.</exception>
    IAsyncEnumerable<FetchedListing> FetchAsync(SourceFetchContext context, CancellationToken cancellationToken);
}

/// <summary>What the orchestrator tells an adapter before a run.</summary>
/// <param name="Source">Source definition, including its JSON configuration.</param>
/// <param name="FullScan">True when every listing must be enumerated (withdrawals are only detected on full scans).</param>
/// <param name="AgencyId">Agency the source belongs to, when it is an agency website.</param>
/// <param name="StoredFingerprint">Returns the fingerprint stored for an external id, or null when the listing is unknown; lets adapters skip detail pages for unchanged listings.</param>
public sealed record SourceFetchContext(Source Source, bool FullScan, Guid? AgencyId, Func<string, CancellationToken, Task<string?>> StoredFingerprint);

/// <summary>One listing seen during a fetch. <see cref="Observation"/> is null when the adapter verified the listing is unchanged and skipped its detail page.</summary>
public sealed record FetchedListing(string ExternalId, ListingObservation? Observation, RawDocument? Raw)
{
    public static FetchedListing Unchanged(string externalId) => new(externalId, null, null);

    public static FetchedListing Of(ListingObservation observation, RawDocument raw) => new(observation.ExternalId, observation, raw);

    public bool IsUnchanged => Observation is null;
}

public sealed record RawDocument(string ContentType, string Body);

public sealed class SourceSchemaChangedException(string sourceKey, string detail)
    : Exception($"Source '{sourceKey}' no longer matches the adapter: {detail}")
{
    public string SourceKey { get; } = sourceKey;
}
