using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace HomeRadar.Sources.Common;

public static class HtmlParsing
{
    private static readonly HtmlParser Parser = new();

    public static IHtmlDocument Parse(string html) => Parser.ParseDocument(html);

    public static string Text(this IElement? element) => Parsing.Collapse(element?.TextContent);

    public static string? Text(this IElement element, string selector) => element.QuerySelector(selector)?.Text();

    public static Uri? Absolute(string? href, Uri baseUrl) =>
        string.IsNullOrWhiteSpace(href) ? null : Uri.TryCreate(baseUrl, href.Trim(), out var uri) ? uri : null;
}
