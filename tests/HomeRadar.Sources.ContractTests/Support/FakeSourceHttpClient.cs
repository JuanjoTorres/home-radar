using System.Net;
using HomeRadar.Sources.Common;

namespace HomeRadar.Sources.ContractTests.Support;

internal sealed class FakeSourceHttpClient : ISourceHttpClient
{
    private readonly List<(Func<Uri, bool> Match, string Source, string Fixture)> _routes = [];

    public List<Uri> Requests { get; } = [];

    public FakeSourceHttpClient Map(Func<Uri, bool> match, string source, string fixture)
    {
        _routes.Add((match, source, fixture));
        return this;
    }

    public Task<FetchedPage> GetAsync(Uri url, CancellationToken cancellationToken)
    {
        Requests.Add(url);
        foreach (var (match, source, fixture) in _routes)
        {
            if (match(url))
            {
                return Task.FromResult(new FetchedPage(url, HttpStatusCode.OK, "text/html", Fixtures.Read(source, fixture)));
            }
        }

        return Task.FromResult(new FetchedPage(url, HttpStatusCode.NotFound, "text/html", string.Empty));
    }
}
