using HomeRadar.Application.Ports;
using HomeRadar.Application.Search;
using Microsoft.EntityFrameworkCore;

namespace HomeRadar.Application.Ingestion;

public sealed class MatchStage(IHomeRadarDbContext db) : IIngestionStage
{
    public int Order => StageOrder.Match;

    public async Task ExecuteAsync(IngestionContext context, CancellationToken cancellationToken)
    {
        if (!context.ShouldContinue)
        {
            return;
        }

        var listing = context.Listing!;
        context.OnWatchlist = await db.WatchlistItems.AsNoTracking().AnyAsync(item => item.ListingId == listing.Id, cancellationToken);

        if (!context.IsNew)
        {
            return;
        }

        var profiles = await db.SearchProfiles.AsNoTracking().Where(profile => profile.Enabled).ToListAsync(cancellationToken);
        foreach (var profile in profiles)
        {
            var predicate = ListingFilterSpecification.Build(profile.Filter, context.Now).Compile();
            if (predicate(listing))
            {
                context.MatchedSearchProfileIds.Add(profile.Id);
            }
        }
    }
}
