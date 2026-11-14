using System.Runtime.CompilerServices;
using System.Text.Json;
using HomeRadar.Application.Ports;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.ValueObjects;
using HomeRadar.Sources.Common;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

namespace HomeRadar.Sources.Diglo;

public sealed record DigloConfiguration(
    string ListUrl = "https://digloservicer.com/venta-activos/cualquiera/islas-baleares",
    int MaxPages = 40,
    bool FetchDetails = true);

/// <summary>Diglo (Santander) is a Drupal site: 0-based <c>page</c> parameter, six cards per page, coordinates and description on the detail page.</summary>
public sealed partial class DigloSource(ISourceHttpClient http, ILogger<DigloSource> logger) : IListingSource
{
    public const string Key = "diglo";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string AdapterKey => Key;

    public async IAsyncEnumerable<FetchedListing> FetchAsync(SourceFetchContext context, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var configuration = JsonSerializer.Deserialize<DigloConfiguration>(context.Source.ConfigurationJson, JsonOptions) ?? new DigloConfiguration();
        var baseUrl = new Uri(configuration.ListUrl);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var page = 0; page < configuration.MaxPages; page++)
        {
            var url = page == 0 ? baseUrl : new Uri($"{configuration.ListUrl}?page={page}");
            var fetched = await http.GetAsync(url, cancellationToken);
            var cards = DigloParser.ParseCards(fetched.Body, baseUrl);
            if (cards.Count == 0)
            {
                if (page == 0 && !fetched.Body.Contains("drupal-settings-json", StringComparison.Ordinal))
                {
                    throw new SourceSchemaChangedException(Key, "listing page has no Drupal settings nor listing blocks");
                }

                yield break;
            }

            foreach (var card in cards.Where(card => seen.Add(card.ExternalId)))
            {
                var fingerprint = ContentHash.Of($"{card.Price?.Amount}|{card.Title}|{card.PhotoUrls.Count}").Value;
                if (configuration.FetchDetails && await context.StoredFingerprint(card.ExternalId, cancellationToken) == fingerprint)
                {
                    yield return FetchedListing.Unchanged(card.ExternalId);
                    continue;
                }

                DigloDetail? detail = null;
                var raw = new RawDocument("text/html", card.CardHtml);
                if (configuration.FetchDetails)
                {
                    var detailPage = await http.GetAsync(card.Url, cancellationToken);
                    if (detailPage.NotFound)
                    {
                        DisappearedBetweenListAndDetail(logger, card.ExternalId);
                        continue;
                    }

                    detail = DigloParser.ParseDetail(detailPage.Body, baseUrl, card.ExternalId);
                    raw = new RawDocument(detailPage.ContentType, detailPage.Body);
                }

                yield return FetchedListing.Of(ToObservation(card, detail, fingerprint), raw);
            }

            if (!context.FullScan)
            {
                yield break;
            }
        }
    }

    private static ListingObservation ToObservation(DigloCard card, DigloDetail? detail, string fingerprint)
    {
        var features = new Dictionary<string, string>(StringComparer.Ordinal) { [FeatureKeys.Fingerprint] = fingerprint };
        if (card.ProductType is { Length: > 0 })
        {
            features[FeatureKeys.Condition] = card.ProductType;
        }

        if (detail?.CadastralParcel is { } parcel)
        {
            features["cadastralParcel"] = parcel;
        }

        Point? location = detail is { Latitude: { } lat, Longitude: { } lon } && lat != 0 ? new Point(lon, lat) { SRID = UtmProjection.Wgs84Srid } : null;
        return new ListingObservation
        {
            ExternalId = card.ExternalId,
            Url = card.Url,
            Title = card.Title,
            Price = card.Price ?? detail?.Price,
            BuiltArea = Area.FromNullable(card.SquareMetres),
            Rooms = card.Rooms,
            Bathrooms = card.Bathrooms,
            PropertyType = Parsing.MapPropertyType(card.ProductType ?? card.Title),
            Municipality = card.Municipality,
            Location = location,
            Description = detail?.Description,
            Features = features,
            PhotoUrls = detail is { PhotoUrls.Count: > 0 } ? detail.PhotoUrls : card.PhotoUrls,
            CadastralReference = detail?.CadastralReference,
        };
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Listing {ExternalId} disappeared between list and detail")]
    private static partial void DisappearedBetweenListAndDetail(ILogger logger, string externalId);
}
