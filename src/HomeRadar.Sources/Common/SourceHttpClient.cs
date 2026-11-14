using System.Net;

namespace HomeRadar.Sources.Common;

/// <summary>Thin wrapper around the named <c>sources</c> HttpClient: one GET per page, with the body and content type kept for snapshots.</summary>
public interface ISourceHttpClient
{
    /// <summary>Downloads a page as text.</summary>
    /// <param name="url">Absolute URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Body, content type and final status.</returns>
    /// <exception cref="HttpRequestException">On non-success status codes other than 404.</exception>
    Task<FetchedPage> GetAsync(Uri url, CancellationToken cancellationToken);
}

public sealed record FetchedPage(Uri Url, HttpStatusCode Status, string ContentType, string Body)
{
    public bool NotFound => Status == HttpStatusCode.NotFound;
}

public sealed class SourceHttpClient(IHttpClientFactory factory) : ISourceHttpClient
{
    public const string ClientName = "homeradar-sources";

    public async Task<FetchedPage> GetAsync(Uri url, CancellationToken cancellationToken)
    {
        using var client = factory.CreateClient(ClientName);
        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new FetchedPage(url, response.StatusCode, "text/html", string.Empty);
        }

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "text/html";
        return new FetchedPage(response.RequestMessage?.RequestUri ?? url, response.StatusCode, contentType, body);
    }
}
