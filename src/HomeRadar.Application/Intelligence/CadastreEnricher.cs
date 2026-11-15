using System.Globalization;
using HomeRadar.Application.Ports;
using HomeRadar.Domain.Intelligence;
using HomeRadar.Domain.Listings;
using Microsoft.Extensions.Logging;

namespace HomeRadar.Application.Intelligence;

public sealed partial class CadastreEnricher(CadastreService cadastre, ILogger<CadastreEnricher> logger) : IListingEnricher
{
    public async Task<IReadOnlyList<EnrichmentFact>> EnrichAsync(Listing listing, CancellationToken cancellationToken)
    {
        if (listing.CadastralMatch is CadastralMatchQuality.Unit or CadastralMatchQuality.Manual)
        {
            return [];
        }

        CadastralFacts? facts;
        try
        {
            facts = await cadastre.ResolveAsync(listing, cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException or TaskCanceledException or FormatException)
        {
            LookupFailed(logger, listing.Id, exception.Message);
            return [];
        }

        if (facts is null)
        {
            return [];
        }

        listing.SetCadastralFacts(facts);
        var confidence = facts.Match == CadastralMatchQuality.Unit ? 0.9m : 0.5m;
        var result = new List<EnrichmentFact> { new("cadastre.match", facts.Match.ToString(), EnrichmentOrigin.Rules, confidence) };
        if (facts.Reference is { } reference)
        {
            result.Add(new EnrichmentFact("cadastre.reference", reference, EnrichmentOrigin.Rules, confidence));
        }

        if (facts.YearBuilt is { } year)
        {
            result.Add(new EnrichmentFact("cadastre.yearBuilt", year.ToString(CultureInfo.InvariantCulture), EnrichmentOrigin.Rules, confidence));
        }

        if (facts.BuiltSquareMetres is { } area)
        {
            result.Add(new EnrichmentFact("cadastre.builtSquareMetres", area.ToString(CultureInfo.InvariantCulture), EnrichmentOrigin.Rules, confidence));
        }

        return result;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Catastro lookup failed for listing {ListingId}: {Reason}")]
    private static partial void LookupFailed(ILogger logger, Guid listingId, string reason);
}
