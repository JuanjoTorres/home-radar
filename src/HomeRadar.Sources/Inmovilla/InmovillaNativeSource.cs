using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using HomeRadar.Application.Ports;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.ValueObjects;
using HomeRadar.Sources.Common;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

namespace HomeRadar.Sources.Inmovilla;

public sealed record InmovillaConfiguration(
    string BaseUrl,
    string ListPath = "venta/es/",
    int PageSize = 48,
    int MaxPages = 60,
    bool FetchDetails = true);

/// <summary>
/// Agencies on a native Inmovilla website embed every listing of the page as JSON (<c>listados.paginacion</c>),
/// including coordinates, previous price and last-change timestamp. Detail pages are fetched only for new or changed
/// listings to pick up the description, year built and condition.
/// </summary>
public sealed partial class InmovillaNativeSource(ISourceHttpClient http, ILogger<InmovillaNativeSource> logger) : IListingSource
{
    public const string Key = "inmovilla";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string AdapterKey => Key;

    public async IAsyncEnumerable<FetchedListing> FetchAsync(SourceFetchContext context, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var configuration = JsonSerializer.Deserialize<InmovillaConfiguration>(context.Source.ConfigurationJson, JsonOptions)
            ?? throw new InvalidOperationException($"Source {context.Source.Key} needs an Inmovilla configuration with baseUrl.");
        var baseUrl = new Uri(configuration.BaseUrl.EndsWith('/') ? configuration.BaseUrl : configuration.BaseUrl + "/");
        var seen = new HashSet<long>();

        for (var page = 1; page <= configuration.MaxPages; page++)
        {
            var url = new Uri(baseUrl, $"{configuration.ListPath}?res={configuration.PageSize}&pag={page}");
            var fetched = await http.GetAsync(url, cancellationToken);
            var (info, listings) = InmovillaParser.ParseListPage(fetched.Body);

            foreach (var listing in listings.Where(listing => listing.Unavailable == 0 && seen.Add(listing.Code)))
            {
                var externalId = listing.Code.ToString(CultureInfo.InvariantCulture);
                var fingerprint = ContentHash.Of($"{listing.ChangedAt}|{listing.Price}|{listing.PreviousPrice}|{listing.PhotoCount}|{listing.Status}").Value;
                if (configuration.FetchDetails && await context.StoredFingerprint(externalId, cancellationToken) == fingerprint)
                {
                    yield return FetchedListing.Unchanged(externalId);
                    continue;
                }

                var detailUrl = HtmlParsing.Absolute(listing.DetailPath, baseUrl) ?? new Uri(baseUrl, $"ficha/{listing.Code}/es/");
                InmovillaListing? detail = null;
                string? description = null;
                var raw = new RawDocument("application/json", listing.RawJson);
                if (configuration.FetchDetails)
                {
                    var detailPage = await http.GetAsync(detailUrl, cancellationToken);
                    if (detailPage.NotFound)
                    {
                        DisappearedBetweenListAndDetail(logger, listing.Code);
                        continue;
                    }

                    detail = InmovillaParser.ParseDetailRecord(detailPage.Body);
                    description = InmovillaParser.ParseDescription(detailPage.Body);
                    raw = new RawDocument(detailPage.ContentType, detailPage.Body);
                }

                yield return FetchedListing.Of(ToObservation(listing, detail, description, detailUrl, fingerprint, context.AgencyId), raw);
            }

            if (!context.FullScan || info.Position + info.Elements > info.Total)
            {
                yield break;
            }
        }
    }

    private static ListingObservation ToObservation(InmovillaListing listing, InmovillaListing? detail, string? description, Uri url, string fingerprint, Guid? agencyId)
    {
        var merged = detail ?? listing;
        var features = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [FeatureKeys.Fingerprint] = fingerprint,
            [FeatureKeys.Elevator] = Flag(merged.Elevator),
            [FeatureKeys.Terrace] = Flag(listing.Terrace),
            [FeatureKeys.Parking] = Flag(listing.Parking),
            [FeatureKeys.Pool] = Flag(listing.CommunityPool + listing.PrivatePool),
        };
        AddIf(features, FeatureKeys.Reference, listing.Reference);
        AddIf(features, FeatureKeys.YearBuilt, merged.YearBuilt is > 1000 ? merged.YearBuilt.Value.ToString(CultureInfo.InvariantCulture) : null);
        AddIf(features, FeatureKeys.Condition, merged.ConditionLabel is { Length: > 0 } and not "Ninguno" ? merged.ConditionLabel : null);
        AddIf(features, FeatureKeys.EnergyRating, merged.EnergyLetter);
        AddIf(features, FeatureKeys.Floor, merged.Floor.ValueKind is JsonValueKind.Number or JsonValueKind.String ? merged.Floor.ToString() : null);
        AddIf(features, FeatureKeys.PostalCode, merged.PostalCode.ValueKind is JsonValueKind.Number or JsonValueKind.String ? merged.PostalCode.ToString() : null);
        AddIf(features, FeatureKeys.PlotSquareMetres, listing.PlotSquareMetres is > 0 ? listing.PlotSquareMetres.Value.ToString(CultureInfo.InvariantCulture) : null);
        if (listing.PreviousPrice is > 0 && listing.Price is > 0 && listing.PreviousPrice > listing.Price)
        {
            features[FeatureKeys.PreviousPrice] = listing.PreviousPrice.Value.ToString(CultureInfo.InvariantCulture);
            features[FeatureKeys.Discount] = Math.Round((listing.PreviousPrice.Value - listing.Price.Value) / listing.PreviousPrice.Value * 100m, 1).ToString(CultureInfo.InvariantCulture);
        }

        if (listing.Exclusive != 0)
        {
            features[FeatureKeys.Exclusive] = "true";
        }

        if (merged.VpoType is > 0 and not 6)
        {
            features[FeatureKeys.Vpo] = "true";
        }

        var photos = Enumerable.Range(1, Math.Max(0, listing.PhotoCount))
            .Select(index => new Uri($"https://fotos{listing.PhotoServer}.apinmo.com/{listing.AgencyNumber}/{listing.Code}/{listing.PhotoLetter}-{index}.jpg"))
            .ToList();
        Point? location = listing is { Latitude: { } lat, Longitude: { } lon } && lat != 0 && lon != 0 ? new Point(lon, lat) { SRID = UtmProjection.Wgs84Srid } : null;
        var status = listing.Status switch
        {
            1 => ListingStatus.Active,
            2 => ListingStatus.Reserved,
            3 => ListingStatus.Sold,
            _ => ListingStatus.Active,
        };

        return new ListingObservation
        {
            ExternalId = listing.Code.ToString(CultureInfo.InvariantCulture),
            Url = url,
            Title = Parsing.Collapse(listing.DefaultTitle ?? listing.AutoTitle ?? $"{listing.TypeLabel} en {listing.City}"),
            Price = listing.PriceOnRequest == 0 && listing.Price is > 0 ? Money.Euros(listing.Price.Value) : null,
            BuiltArea = Area.FromNullable(listing.BuiltSquareMetres),
            UsableArea = Area.FromNullable(listing.UsableSquareMetres),
            Rooms = listing.Rooms,
            Bathrooms = listing.Bathrooms,
            PropertyType = Parsing.MapPropertyType(listing.TypeLabel),
            Municipality = listing.City?.Trim(),
            Zone = listing.Zone?.Trim(),
            Location = location,
            Description = description,
            Features = features,
            PhotoUrls = photos,
            Status = status,
            PublishedAt = InmovillaParser.ParseTimestamp(listing.CreatedAt),
            UpdatedAt = InmovillaParser.ParseTimestamp(listing.ChangedAt),
            AgencyId = agencyId,
        };
    }

    private static string Flag(int value) => value > 0 ? "true" : "false";

    private static void AddIf(Dictionary<string, string> features, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            features[key] = value;
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Listing {ExternalId} disappeared between list and detail")]
    private static partial void DisappearedBetweenListAndDetail(ILogger logger, long externalId);
}
