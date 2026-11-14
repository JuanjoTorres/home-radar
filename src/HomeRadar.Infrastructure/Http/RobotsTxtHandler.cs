using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace HomeRadar.Infrastructure.Http;

public sealed class RobotsDisallowedException(Uri url) : Exception($"robots.txt disallows {url}")
{
    public Uri Url { get; } = url;
}

/// <summary>Fetches and caches robots.txt per host and refuses requests it disallows; exposes the crawl delay to the rate limiter.</summary>
public sealed class RobotsTxtHandler(RobotsCache cache, SourceHttpOptions options, ILogger<RobotsTxtHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var url = request.RequestUri ?? throw new InvalidOperationException("Request has no URI.");
        if (url.AbsolutePath.Equals("/robots.txt", StringComparison.OrdinalIgnoreCase))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var rules = await cache.GetAsync(url, host => FetchRulesAsync(host, cancellationToken));
        if (!rules.IsAllowed(url.PathAndQuery))
        {
            logger.LogWarning("robots.txt of {Host} disallows {Path}", url.Host, url.PathAndQuery);
            throw new RobotsDisallowedException(url);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<RobotsRules> FetchRulesAsync(Uri host, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(host, "/robots.txt"));
        try
        {
            using var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode is System.Net.HttpStatusCode.NotFound or System.Net.HttpStatusCode.Gone)
            {
                return RobotsRules.AllowAll;
            }

            if (!response.IsSuccessStatusCode)
            {
                return RobotsRules.Parse("User-agent: *\nDisallow: /", options.UserAgentToken);
            }

            return RobotsRules.Parse(await response.Content.ReadAsStringAsync(cancellationToken), options.UserAgentToken);
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "robots.txt of {Host} could not be fetched; treating as disallow-all for this run", host.Host);
            return RobotsRules.Parse("User-agent: *\nDisallow: /", options.UserAgentToken);
        }
    }
}

public sealed class RobotsCache(TimeProvider clock)
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromHours(12);

    private readonly ConcurrentDictionary<string, Lazy<Task<(RobotsRules Rules, DateTimeOffset FetchedAt)>>> _entries = new(StringComparer.OrdinalIgnoreCase);

    public async Task<RobotsRules> GetAsync(Uri url, Func<Uri, Task<RobotsRules>> fetch)
    {
        var host = new Uri(url.GetLeftPart(UriPartial.Authority));
        while (true)
        {
            var entry = _entries.GetOrAdd(host.Host, _ => new Lazy<Task<(RobotsRules, DateTimeOffset)>>(async () => (await fetch(host), clock.GetUtcNow())));
            try
            {
                var (rules, fetchedAt) = await entry.Value;
                if (clock.GetUtcNow() - fetchedAt < Lifetime)
                {
                    return rules;
                }

                _entries.TryRemove(new KeyValuePair<string, Lazy<Task<(RobotsRules, DateTimeOffset)>>>(host.Host, entry));
            }
            catch
            {
                _entries.TryRemove(new KeyValuePair<string, Lazy<Task<(RobotsRules, DateTimeOffset)>>>(host.Host, entry));
                throw;
            }
        }
    }

    public TimeSpan? CrawlDelayFor(string host) =>
        _entries.TryGetValue(host, out var entry) && entry.IsValueCreated && entry.Value.IsCompletedSuccessfully ? entry.Value.Result.Rules.CrawlDelay : null;
}
