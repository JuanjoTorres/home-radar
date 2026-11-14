using System.Runtime.CompilerServices;
using System.Text.Json;
using HomeRadar.Application.Ports;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.ValueObjects;
using HomeRadar.Sources.Common;
using Microsoft.Extensions.Logging;

namespace HomeRadar.Sources.Solvia;

public sealed record SolviaConfiguration(
    string ListUrl = "https://www.solvia.es/es/comprar/viviendas/balears-illes",
    int PageSize = 20,
    decimal MaxPrice = 5_000_000m,
    decimal MinBandWidth = 5_000m,
    bool FetchDetails = true);

/// <summary>
/// Solvia publishes server-rendered listing pages without a working page parameter, so a full scan slices the
/// province by price band (bisecting any band that fills a page) and a quick scan reads only the default first page.
/// Detail pages carry the full property record in the Angular transfer state and are fetched only when the card fingerprint changed.
/// </summary>
public sealed partial class SolviaSource(ISourceHttpClient http, ILogger<SolviaSource> logger) : IListingSource
{
    public const string Key = "solvia";

    public string AdapterKey => Key;

    public async IAsyncEnumerable<FetchedListing> FetchAsync(SourceFetchContext context, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var configuration = JsonSerializer.Deserialize<SolviaConfiguration>(context.Source.ConfigurationJson, JsonOptions) ?? new SolviaConfiguration();
        var listUrl = new Uri(configuration.ListUrl);
        var seen = new HashSet<string>(StringComparer.Ordinal);

        await foreach (var card in EnumerateCardsAsync(listUrl, configuration, context.FullScan, cancellationToken))
        {
            if (!seen.Add(card.ExternalId))
            {
                continue;
            }

            var fingerprint = Fingerprint(card);
            if (configuration.FetchDetails && await context.StoredFingerprint(card.ExternalId, cancellationToken) == fingerprint)
            {
                yield return FetchedListing.Unchanged(card.ExternalId);
                continue;
            }

            if (!configuration.FetchDetails)
            {
                yield return FetchedListing.Of(FromCard(card, fingerprint), new RawDocument("text/html", card.CardHtml));
                continue;
            }

            var detailPage = await http.GetAsync(card.Url, cancellationToken);
            if (detailPage.NotFound)
            {
                DisappearedBetweenListAndDetail(logger, card.ExternalId);
                continue;
            }

            var detail = SolviaDetailParser.Parse(detailPage.Body);
            yield return FetchedListing.Of(FromDetail(card, detail, fingerprint), new RawDocument(detailPage.ContentType, detailPage.Body));
        }
    }

    private async IAsyncEnumerable<SolviaCard> EnumerateCardsAsync(Uri listUrl, SolviaConfiguration configuration, bool fullScan, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!fullScan)
        {
            var page = await http.GetAsync(listUrl, cancellationToken);
            foreach (var card in ParseOrThrow(page.Body, listUrl))
            {
                yield return card;
            }

            yield break;
        }

        var bands = new Stack<(decimal Min, decimal Max)>();
        bands.Push((0m, configuration.MaxPrice));
        while (bands.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var (min, max) = bands.Pop();
            var bandUrl = new Uri($"{listUrl}?filtro.minPrecio=:ge:{min:0}&filtro.maxPrecio=:le:{max:0}");
            var page = await http.GetAsync(bandUrl, cancellationToken);
            var cards = ParseOrThrow(page.Body, listUrl);

            if (cards.Count >= configuration.PageSize && max - min > configuration.MinBandWidth)
            {
                var middle = Math.Floor((min + max) / 2);
                bands.Push((middle + 1, max));
                bands.Push((min, middle));
                continue;
            }

            foreach (var card in cards)
            {
                yield return card;
            }
        }
    }

    private static IReadOnlyList<SolviaCard> ParseOrThrow(string html, Uri baseUrl)
    {
        var cards = SolviaListParser.ParseCards(html, baseUrl);
        if (cards.Count == 0 && !html.Contains("house-card", StringComparison.Ordinal) && !html.Contains("ng-state", StringComparison.Ordinal))
        {
            throw new SourceSchemaChangedException(Key, "listing page contains neither house cards nor Angular state");
        }

        return cards;
    }

    private static string Fingerprint(SolviaCard card) =>
        ContentHash.Of(string.Join("|", card.Price?.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture), card.PreviousPrice?.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture), card.Title, string.Join(",", card.Badges), card.PhotoUrl)).Value;

    private static ListingObservation FromCard(SolviaCard card, string fingerprint)
    {
        var features = new Dictionary<string, string>(StringComparer.Ordinal) { [FeatureKeys.Fingerprint] = fingerprint };
        AddCardFeatures(features, card);
        return new ListingObservation
        {
            ExternalId = card.ExternalId,
            Url = card.Url,
            Title = card.Title,
            Price = card.Price,
            BuiltArea = Area.FromNullable(card.BuiltSquareMetres),
            Rooms = card.Rooms,
            Bathrooms = card.Bathrooms,
            PropertyType = Parsing.MapPropertyType(card.Url.AbsolutePath.Split('/').LastOrDefault()),
            Municipality = card.Municipality,
            Zone = card.Address,
            Features = features,
            PhotoUrls = card.PhotoUrl is null ? [] : [card.PhotoUrl],
            Possession = PossessionOf(card.Badges, 0),
        };
    }

    private static ListingObservation FromDetail(SolviaCard card, SolviaPropertyDetail detail, string fingerprint)
    {
        var features = new Dictionary<string, string>(StringComparer.Ordinal) { [FeatureKeys.Fingerprint] = fingerprint };
        AddCardFeatures(features, card);
        var characteristics = detail.Characteristics;
        AddIf(features, FeatureKeys.Condition, characteristics?.Condition ?? card.Condition);
        AddIf(features, FeatureKeys.PostalCode, detail.PostalCode);
        AddIf(features, FeatureKeys.Address, detail.Address?.Trim());
        AddIf(features, FeatureKeys.Origin, detail.Origin);
        AddIf(features, FeatureKeys.Campaign, detail.Campaign?.Name?.Trim());
        if (characteristics is not null)
        {
            features[FeatureKeys.Pool] = characteristics.Pool ? "true" : "false";
            features[FeatureKeys.Parking] = characteristics.Garage ? "true" : "false";
            features[FeatureKeys.StorageRoom] = characteristics.StorageRoom ? "true" : "false";
            features[FeatureKeys.Vpo] = characteristics.Vpo ? "true" : "false";
            AddIf(features, FeatureKeys.Ibi, characteristics.Ibi?.ToString(System.Globalization.CultureInfo.InvariantCulture));
            AddIf(features, FeatureKeys.CommunityFees, characteristics.CommunityFees?.ToString(System.Globalization.CultureInfo.InvariantCulture));
            if (string.Equals(characteristics.Condition, "Obra Nueva", StringComparison.OrdinalIgnoreCase))
            {
                features[FeatureKeys.NewBuild] = "true";
            }
        }

        features[FeatureKeys.SpecialSituation] = detail.SpecialSituation.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var reserved = string.Equals(detail.Reserved, "S", StringComparison.OrdinalIgnoreCase) || characteristics?.Reserved == true;
        CadastralReference? cadastral = CadastralReference.TryParse(characteristics?.CadastralReference, out var reference) ? reference : null;
        var photos = detail.Images.Select(image => image.Url).Where(url => !string.IsNullOrWhiteSpace(url)).Select(url => new Uri(url!.Replace('\\', '/'))).ToList();

        return new ListingObservation
        {
            ExternalId = card.ExternalId,
            Url = card.Url,
            Title = Parsing.Collapse(detail.Title) is { Length: > 0 } title ? title : card.Title,
            Price = detail.Price is > 0 ? Money.Euros(detail.Price.Value) : card.Price,
            BuiltArea = Area.FromNullable(detail.SquareMetres ?? card.BuiltSquareMetres),
            Rooms = detail.Rooms ?? card.Rooms,
            Bathrooms = detail.Bathrooms ?? card.Bathrooms,
            PropertyType = Parsing.MapPropertyType(detail.PropertyType?.Name ?? card.Url.AbsolutePath),
            Municipality = detail.Municipality?.Name?.Trim() ?? card.Municipality,
            Zone = detail.Address?.Trim() ?? card.Address,
            Description = detail.Description?.Replace("\r\n", "\n", StringComparison.Ordinal).Trim(),
            Features = features,
            PhotoUrls = photos.Count > 0 ? photos : card.PhotoUrl is null ? [] : [card.PhotoUrl],
            Status = reserved ? ListingStatus.Reserved : ListingStatus.Active,
            Possession = PossessionOf(card.Badges, detail.SpecialSituation),
            CadastralReference = cadastral,
            PublishedAt = SolviaDetailParser.ParseSpanishTimestamp(detail.FirstPublishedAt),
            UpdatedAt = SolviaDetailParser.ParseSpanishTimestamp(detail.UpdatedAt),
        };
    }

    private static void AddCardFeatures(Dictionary<string, string> features, SolviaCard card)
    {
        AddIf(features, FeatureKeys.PreviousPrice, card.PreviousPrice?.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture));
        if (card.PreviousPrice is { } before && card.Price is { } after && before.Amount > 0)
        {
            features[FeatureKeys.Discount] = Math.Round((before.Amount - after.Amount) / before.Amount * 100m, 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        if (card.Badges.Count > 0)
        {
            features[FeatureKeys.Badges] = string.Join(" | ", card.Badges);
        }
    }

    private static PossessionStatus PossessionOf(IReadOnlyList<string> badges, int specialSituation) =>
        specialSituation != 0 || badges.Any(badge => badge.Contains("SITUACIÓN ESPECIAL", StringComparison.OrdinalIgnoreCase)) ? PossessionStatus.Occupied : PossessionStatus.Unknown;

    private static void AddIf(Dictionary<string, string> features, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            features[key] = value;
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [LoggerMessage(Level = LogLevel.Information, Message = "Listing {ExternalId} disappeared between list and detail")]
    private static partial void DisappearedBetweenListAndDetail(ILogger logger, string externalId);
}
