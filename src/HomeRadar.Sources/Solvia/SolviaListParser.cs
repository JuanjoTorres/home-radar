using System.Text.RegularExpressions;
using AngleSharp.Dom;
using HomeRadar.Domain.ValueObjects;
using HomeRadar.Sources.Common;

namespace HomeRadar.Sources.Solvia;

public sealed record SolviaCard(
    string ExternalId,
    Uri Url,
    string Title,
    string? Municipality,
    string? Address,
    Money? Price,
    Money? PreviousPrice,
    decimal? BuiltSquareMetres,
    int? Rooms,
    int? Bathrooms,
    string? Condition,
    IReadOnlyList<string> Badges,
    Uri? PhotoUrl,
    string CardHtml);

public static partial class SolviaListParser
{
    public static int? ParseTotal(string html)
    {
        var match = TotalPattern().Match(html);
        return match.Success ? Parsing.ParseInt(match.Groups[1].Value) : null;
    }

    public static IReadOnlyList<SolviaCard> ParseCards(string html, Uri baseUrl)
    {
        var document = HtmlParsing.Parse(html);
        var cards = new List<SolviaCard>();
        foreach (var figure in document.QuerySelectorAll("figure.house-card"))
        {
            var link = figure.QuerySelector("a[href*='/es/propiedades/comprar/']");
            var url = HtmlParsing.Absolute(link?.GetAttribute("href"), baseUrl);
            if (url is null)
            {
                continue;
            }

            var idMatch = IdPattern().Match(url.AbsolutePath);
            if (!idMatch.Success)
            {
                continue;
            }

            var addressParts = figure.QuerySelectorAll(".build-address span").Select(span => span.Text()).Where(text => text.Length > 0).ToList();
            var tags = figure.QuerySelectorAll("ul.build-tags li").ToList();
            cards.Add(new SolviaCard(
                ExternalId: $"{idMatch.Groups[1].Value}-{idMatch.Groups[2].Value}",
                Url: url,
                Title: figure.Text("h3.build-name") ?? string.Empty,
                Municipality: addressParts.Skip(1).FirstOrDefault()?.TrimStart(',', ' '),
                Address: addressParts.Skip(2).FirstOrDefault()?.TrimStart('-', ' '),
                Price: Parsing.ParseEuros(figure.Text(".final-price")),
                PreviousPrice: Parsing.ParseEuros(figure.Text(".previous-price")),
                BuiltSquareMetres: Parsing.ParseDecimal(tags.FirstOrDefault(li => li.QuerySelector(".icon-medidas") is not null)?.Text()),
                Rooms: Parsing.ParseInt(tags.FirstOrDefault(li => li.QuerySelector(".icon-dormitorio") is not null)?.Text()),
                Bathrooms: Parsing.ParseInt(tags.FirstOrDefault(li => li.QuerySelector(".icon-bano") is not null)?.Text()),
                Condition: tags.FirstOrDefault(li => li.ClassList.Contains("last-item"))?.Text(),
                Badges: figure.QuerySelectorAll("li.etiqueta").Select(li => li.Text()).Where(text => text.Length > 0).Distinct(StringComparer.Ordinal).ToList(),
                PhotoUrl: HtmlParsing.Absolute(figure.QuerySelector("img.az-element-mosaic__image")?.GetAttribute("src")?.Replace('\\', '/'), baseUrl),
                CardHtml: figure.OuterHtml));
        }

        return cards;
    }

    [GeneratedRegex(@"-(\d+)-(\d+)$")]
    private static partial Regex IdPattern();

    [GeneratedRegex(@"(\d+)\s+(?:Viviendas|Inmuebles|Pisos|Casas)", RegexOptions.IgnoreCase)]
    private static partial Regex TotalPattern();
}
