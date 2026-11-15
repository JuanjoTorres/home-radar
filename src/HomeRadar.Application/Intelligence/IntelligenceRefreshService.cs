using HomeRadar.Application.Analysis;
using HomeRadar.Application.Ports;
using HomeRadar.Domain.Listings;
using Microsoft.EntityFrameworkCore;

namespace HomeRadar.Application.Intelligence;

public sealed class IntelligenceRefreshService(IHomeRadarDbContext db, AnalysisService analysis, ListingInsightService insights)
{
    private const int BatchSize = 100;

    public async Task<int> RefreshActiveAsync(CancellationToken cancellationToken)
    {
        var profile = await analysis.ResolveProfileAsync(null, cancellationToken);
        var defaultProfile = profile.IsSuccess ? profile.Value : null;
        var ids = await db.Listings.AsNoTracking().Where(listing => listing.Status == ListingStatus.Active).OrderBy(listing => listing.Id).Select(listing => listing.Id).ToListAsync(cancellationToken);
        var refreshed = 0;
        foreach (var batch in ids.Chunk(BatchSize))
        {
            var listings = await db.Listings.Where(listing => batch.Contains(listing.Id)).ToListAsync(cancellationToken);
            foreach (var listing in listings)
            {
                if ((await insights.RefreshAsync(listing, defaultProfile, cancellationToken)).IsSuccess)
                {
                    refreshed++;
                }
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        return refreshed;
    }
}
