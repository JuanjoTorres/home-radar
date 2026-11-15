using HomeRadar.Application.Ports;
using HomeRadar.Domain.Finance;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.Sources;

namespace HomeRadar.Application.Ingestion;

public sealed class IngestionContext(Source source, SourceRun run, FetchedListing fetched, DateTimeOffset now)
{
    public Source Source { get; } = source;

    public SourceRun Run { get; } = run;

    public FetchedListing Fetched { get; } = fetched;

    public DateTimeOffset Now { get; } = now;

    public Listing? Listing { get; set; }

    public bool IsNew { get; set; }

    public bool Changed { get; set; }

    public List<ListingEvent> NewEvents { get; } = [];

    public List<Guid> MatchedSearchProfileIds { get; } = [];

    public bool OnWatchlist { get; set; }

    public FinancialAnalysis? Analysis { get; set; }

    public bool ShouldContinue => Listing is not null && (IsNew || Changed);
}
