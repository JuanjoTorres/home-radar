using System.Collections.Concurrent;

namespace HomeRadar.Infrastructure.Http;

/// <summary>Serialises requests per host and enforces a minimum interval: the larger of the configured delay and the host's robots.txt crawl-delay.</summary>
public sealed class PerHostRateLimitHandler(RobotsCache robots, SourceHttpOptions options, TimeProvider clock) : DelegatingHandler
{
    private readonly ConcurrentDictionary<string, HostGate> _gates = new(StringComparer.OrdinalIgnoreCase);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var host = request.RequestUri?.Host ?? throw new InvalidOperationException("Request has no URI.");
        var gate = _gates.GetOrAdd(host, _ => new HostGate());
        await gate.Lock.WaitAsync(cancellationToken);
        try
        {
            var interval = Max(options.MinimumInterval, robots.CrawlDelayFor(host) ?? TimeSpan.Zero);
            var wait = gate.LastRequestAt + interval - clock.GetUtcNow();
            if (wait > TimeSpan.Zero)
            {
                await Task.Delay(wait, clock, cancellationToken);
            }

            gate.LastRequestAt = clock.GetUtcNow();
            return await base.SendAsync(request, cancellationToken);
        }
        finally
        {
            gate.Lock.Release();
        }
    }

    private static TimeSpan Max(TimeSpan left, TimeSpan right) => left > right ? left : right;

    private sealed class HostGate
    {
        public SemaphoreSlim Lock { get; } = new(1, 1);

        public DateTimeOffset LastRequestAt { get; set; } = DateTimeOffset.MinValue;
    }
}
