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

public sealed record SourceFetchContext(Source Source, bool FullScan, Guid? AgencyId);

public sealed record FetchedListing(ListingObservation Observation, RawDocument Raw);

public sealed record RawDocument(string ContentType, string Body);

public sealed class SourceSchemaChangedException(string sourceKey, string detail)
    : Exception($"Source '{sourceKey}' no longer matches the adapter: {detail}")
{
    public string SourceKey { get; } = sourceKey;
}
