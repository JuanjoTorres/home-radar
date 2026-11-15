using HomeRadar.Application.Ports;
using HomeRadar.Domain.Intelligence;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace HomeRadar.Application.Intelligence;

public sealed record ComparableCriteria(Guid? ExcludeListingId, PropertyType PropertyType, decimal? BuiltSquareMetres, NetTopologySuite.Geometries.Point? LocationUtm, string? Municipality);

public sealed class ComparablesService(IHomeRadarDbContext db)
{
    private static readonly PropertyType[] Flats = [PropertyType.Flat, PropertyType.Penthouse, PropertyType.Studio];
    private static readonly PropertyType[] Houses = [PropertyType.House, PropertyType.Townhouse, PropertyType.Finca];
    private const decimal MinimumArea = 20m;

    public Task<MarketSnapshot?> SnapshotAsync(Listing listing, ComparablesConfig config, DateTimeOffset now, CancellationToken cancellationToken) =>
        SnapshotAsync(new ComparableCriteria(listing.Id, listing.PropertyType, listing.BuiltSquareMetres, listing.LocationUtm, listing.Municipality), config, now, cancellationToken);

    public async Task<MarketSnapshot?> SnapshotAsync(ComparableCriteria listing, ComparablesConfig config, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var excluded = listing.ExcludeListingId ?? Guid.Empty;
        var types = Flats.Contains(listing.PropertyType) ? Flats : Houses.Contains(listing.PropertyType) ? Houses : [listing.PropertyType];
        var since = now.AddDays(-config.MaximumAgeDays);
        var query = db.Listings.AsNoTracking()
            .Where(candidate => candidate.Id != excluded
                && candidate.Status == ListingStatus.Active
                && candidate.PriceAmount != null
                && candidate.BuiltSquareMetres > MinimumArea
                && candidate.LastSeenAt >= since
                && types.Contains(candidate.PropertyType));
        if (listing.BuiltSquareMetres is { } area && area > MinimumArea)
        {
            var low = area * (1m - (config.AreaTolerancePercent / 100m));
            var high = area * (1m + (config.AreaTolerancePercent / 100m));
            query = query.Where(candidate => candidate.BuiltSquareMetres >= low && candidate.BuiltSquareMetres <= high);
        }

        if (listing.LocationUtm is { } point)
        {
            var radius = (double)config.RadiusMetres;
            var nearby = await PointsAsync(query.Where(candidate => candidate.LocationUtm != null && candidate.LocationUtm.IsWithinDistance(point, radius)), cancellationToken);
            if (MarketStatistics.Build(nearby, ComparableScope.Radius, config) is { } snapshot)
            {
                return snapshot;
            }
        }

        if (listing.Municipality is { } municipality)
        {
            var sameMunicipality = await PointsAsync(SameMunicipality(query, municipality), cancellationToken);
            if (MarketStatistics.Build(sameMunicipality, ComparableScope.Municipality, config) is { } snapshot)
            {
                return snapshot;
            }
        }

        return MarketStatistics.Build(await PointsAsync(query, cancellationToken), ComparableScope.Island, config);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862", Justification = "Translated to SQL lower(); StringComparison overloads cannot be translated.")]
    private static IQueryable<Listing> SameMunicipality(IQueryable<Listing> query, string municipality)
    {
        var key = municipality.ToLowerInvariant();
        return query.Where(candidate => candidate.Municipality != null && candidate.Municipality.ToLower() == key);
    }

    private static async Task<List<ComparablePoint>> PointsAsync(IQueryable<Listing> query, CancellationToken cancellationToken)
    {
        var rows = await query
            .Select(candidate => new { candidate.Id, candidate.PriceAmount, candidate.BuiltSquareMetres, candidate.RenovationLevel })
            .Take(2_000)
            .ToListAsync(cancellationToken);
        return rows.Select(row => new ComparablePoint(row.Id, Math.Round(row.PriceAmount!.Value / row.BuiltSquareMetres!.Value, 2, MidpointRounding.ToEven), row.RenovationLevel)).ToList();
    }
}
