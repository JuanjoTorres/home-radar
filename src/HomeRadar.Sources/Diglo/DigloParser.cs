using System.Text.Json;
using System.Text.RegularExpressions;
using HomeRadar.Domain.ValueObjects;
using HomeRadar.Sources.Common;

namespace HomeRadar.Sources.Diglo;

public sealed record DigloCard(string ExternalId, Uri Url, string Title, string? Municipality, string? ProductType, Money? Price, decimal? SquareMetres, int? Rooms, int? Bathrooms, IReadOnlyList<Uri> PhotoUrls, string CardHtml);

public sealed record DigloDetail(string? Description, double? Latitude, double? Longitude, IReadOnlyList<Uri> PhotoUrls, Money? Price, CadastralReference? CadastralReference, string? CadastralParcel);

public static partial class DigloParser
{
    public static int? ParseTotalRows(string html)
    {
        var settings = DrupalSettings(html);
        return settings is not null && settings.Value.TryGetProperty("view_total_rows", out var total) ? Parsing.ParseInt(total.ToString()) : null;
    }

    public static IReadOnlyList<DigloCard> ParseCards(string html, Uri baseUrl)
    {
        var document = HtmlParsing.Parse(html);
        var cards = new List<DigloCard>();
        foreach (var block in document.QuerySelectorAll("div.listing-block"))
        {
            var link = block.QuerySelector("a[rel='bookmark'][href]") ?? block.QuerySelector("a[href*='/venta-']");
            var url = HtmlParsing.Absolute(link?.GetAttribute("href"), baseUrl);
            if (url is null)
            {
                continue;
            }

            var externalId = url.AbsolutePath.TrimEnd('/').Split('/').Last().ToUpperInvariant();
            var photos = block.QuerySelectorAll("img.thumb-image")
                .Select(img => HtmlParsing.Absolute(img.GetAttribute("src"), baseUrl))
                .Where(uri => uri is not null)
                .Select(uri => uri!)
                .Distinct()
                .ToList();
            var icons = block.QuerySelectorAll(".info-icon").ToList();
            cards.Add(new DigloCard(
                externalId,
                url,
                block.Text(".web-publication-name") ?? link?.GetAttribute("title") ?? externalId,
                block.Text(".city-municipio")?.TrimEnd(','),
                block.QuerySelector(".listing-info")?.GetAttribute("product-type"),
                Parsing.ParseEuros(block.Text(".price")),
                Parsing.ParseDecimal(icons.FirstOrDefault(icon => icon.InnerHtml.Contains("superficie", StringComparison.OrdinalIgnoreCase))?.Text()),
                Parsing.ParseInt(icons.FirstOrDefault(icon => icon.InnerHtml.Contains("habitacion", StringComparison.OrdinalIgnoreCase) || icon.InnerHtml.Contains("dormitorio", StringComparison.OrdinalIgnoreCase))?.Text()),
                Parsing.ParseInt(icons.FirstOrDefault(icon => icon.InnerHtml.Contains("bano", StringComparison.OrdinalIgnoreCase) || icon.InnerHtml.Contains("baño", StringComparison.OrdinalIgnoreCase))?.Text()),
                photos,
                block.OuterHtml));
        }

        return cards;
    }

    public static DigloDetail ParseDetail(string html, Uri baseUrl, string externalId)
    {
        var document = HtmlParsing.Parse(html);
        var description = document.QuerySelector(".field--name-field-yera-descripcion")?.Text();
        var settings = DrupalSettings(html);
        double? latitude = settings is not null && settings.Value.TryGetProperty("yera_producto_lat", out var lat) && double.TryParse(lat.ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var latValue) ? latValue : null;
        double? longitude = settings is not null && settings.Value.TryGetProperty("yera_producto_lon", out var lon) && double.TryParse(lon.ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lonValue) ? lonValue : null;
        var photos = document.QuerySelectorAll("img[src*='/imagenes/activos/']")
            .Select(img => HtmlParsing.Absolute(img.GetAttribute("src"), baseUrl))
            .Where(uri => uri is not null && uri.AbsolutePath.Contains($"/{externalId}/", StringComparison.OrdinalIgnoreCase))
            .Select(uri => uri!)
            .Distinct()
            .ToList();
        var cadastralMatch = description is null ? null : CadastralPattern().Match(description);
        CadastralReference? cadastral = cadastralMatch is { Success: true } && CadastralReference.TryParse(cadastralMatch.Value, out var reference) ? reference : null;
        var parcel = cadastralMatch is { Success: true } ? cadastralMatch.Value[..14] : null;
        return new DigloDetail(description, latitude, longitude, photos, Parsing.ParseEuros(document.QuerySelector(".price")?.Text()), cadastral, parcel);
    }

    private static JsonElement? DrupalSettings(string html)
    {
        var match = SettingsPattern().Match(html);
        if (!match.Success)
        {
            return null;
        }

        using var document = JsonDocument.Parse(match.Groups[1].Value);
        return document.RootElement.Clone();
    }

    [GeneratedRegex("<script type=\"application/json\" data-drupal-selector=\"drupal-settings-json\">(.*?)</script>", RegexOptions.Singleline)]
    private static partial Regex SettingsPattern();

    [GeneratedRegex(@"\b\d{7}[A-Z]{2}\d{4}[A-Z](?:\d{4}[A-Z]{2}|\d{4})?\b")]
    private static partial Regex CadastralPattern();
}
