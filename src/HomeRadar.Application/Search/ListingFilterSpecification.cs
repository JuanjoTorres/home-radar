using System.Linq.Expressions;
using HomeRadar.Domain.Intelligence;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.ValueObjects;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace HomeRadar.Application.Search;

/// <summary>
/// Translates a <see cref="ListingFilter"/> into one expression that EF Core compiles to SQL for searches
/// and that is compiled in memory to decide whether a freshly ingested listing matches a saved search.
/// Both paths must agree; <c>ListingFilterSpecificationTests</c> enforces it.
/// </summary>
public static class ListingFilterSpecification
{
    private static readonly WKTReader WktReader = new();

    public static Expression<Func<Listing, bool>> Build(ListingFilter filter, DateTimeOffset now, AffordablePriceLimits? affordability = null)
    {
        if (filter.Affordability is not null && affordability is null)
        {
            throw new InvalidOperationException("The filter has affordability criteria; resolve them with AffordabilityLimitResolver first.");
        }

        var predicate = PredicateComposer.True<Listing>();

        if (filter.Statuses.Count > 0)
        {
            var statuses = filter.Statuses.ToList();
            predicate = predicate.And(l => statuses.Contains(l.Status));
        }

        if (filter.PriceMin is { } priceMin)
        {
            predicate = predicate.And(l => l.PriceAmount != null && l.PriceAmount >= priceMin);
        }

        if (filter.PriceMax is { } priceMax)
        {
            predicate = predicate.And(l => l.PriceAmount != null && l.PriceAmount <= priceMax);
        }

        if (filter.PricePerM2Min is { } ppm2Min)
        {
            predicate = predicate.And(l => l.PriceAmount != null && l.BuiltSquareMetres != null && l.BuiltSquareMetres > 0 && l.PriceAmount / l.BuiltSquareMetres >= ppm2Min);
        }

        if (filter.PricePerM2Max is { } ppm2Max)
        {
            predicate = predicate.And(l => l.PriceAmount != null && l.BuiltSquareMetres != null && l.BuiltSquareMetres > 0 && l.PriceAmount / l.BuiltSquareMetres <= ppm2Max);
        }

        if (filter.BuiltM2Min is { } m2Min)
        {
            predicate = predicate.And(l => l.BuiltSquareMetres != null && l.BuiltSquareMetres >= m2Min);
        }

        if (filter.BuiltM2Max is { } m2Max)
        {
            predicate = predicate.And(l => l.BuiltSquareMetres != null && l.BuiltSquareMetres <= m2Max);
        }

        if (filter.RoomsMin is { } roomsMin)
        {
            predicate = predicate.And(l => l.Rooms != null && l.Rooms >= roomsMin);
        }

        if (filter.RoomsMax is { } roomsMax)
        {
            predicate = predicate.And(l => l.Rooms != null && l.Rooms <= roomsMax);
        }

        if (filter.BathroomsMin is { } bathroomsMin)
        {
            predicate = predicate.And(l => l.Bathrooms != null && l.Bathrooms >= bathroomsMin);
        }

        if (filter.PropertyTypes.Count > 0)
        {
            var types = filter.PropertyTypes.ToList();
            predicate = predicate.And(l => types.Contains(l.PropertyType));
        }

        if (filter.Municipalities.Count > 0)
        {
            var municipalities = filter.Municipalities.Select(Normalize).ToList();
            predicate = predicate.And(l => l.Municipality != null && municipalities.Contains(l.Municipality.ToLower()));
        }

        if (filter.PolygonWkt is { Length: > 0 } wkt)
        {
            var polygon = WktReader.Read(wkt);
            polygon.SRID = UtmProjection.Wgs84Srid;
            predicate = predicate.And(l => l.Location != null && l.Location.Within(polygon));
        }

        if (filter.Radius is { } radius)
        {
            var (easting, northing) = UtmProjection.ToUtm31N(radius.Latitude, radius.Longitude);
            var centre = new Point(easting, northing) { SRID = UtmProjection.Utm31NSrid };
            double metres = radius.Metres;
            predicate = predicate.And(l => l.LocationUtm != null && l.LocationUtm.Distance(centre) <= metres);
        }

        if (filter.SellerTypes.Count > 0)
        {
            var sellerTypes = filter.SellerTypes.ToList();
            predicate = predicate.And(l => sellerTypes.Contains(l.SellerType));
        }

        if (filter.SourceKeys.Count > 0)
        {
            var keys = filter.SourceKeys.ToList();
            predicate = predicate.And(l => keys.Contains(l.SourceKey));
        }

        if (filter.AgencyIds.Count > 0)
        {
            var agencies = filter.AgencyIds.ToList();
            predicate = predicate.And(l => l.AgencyId != null && agencies.Contains(l.AgencyId.Value));
        }

        if (filter.YearBuiltMin is { } yearMin)
        {
            predicate = predicate.And(l => (l.CadastralYearBuilt ?? l.YearBuilt) != null && (l.CadastralYearBuilt ?? l.YearBuilt) >= yearMin);
        }

        if (filter.YearBuiltMax is { } yearMax)
        {
            predicate = predicate.And(l => (l.CadastralYearBuilt ?? l.YearBuilt) != null && (l.CadastralYearBuilt ?? l.YearBuilt) <= yearMax);
        }

        if (filter.DaysOnMarketMin is { } domMin)
        {
            var threshold = now.AddDays(-domMin);
            predicate = predicate.And(l => (l.PublishedAt ?? l.FirstSeenAt) <= threshold);
        }

        if (filter.DaysOnMarketMax is { } domMax)
        {
            var threshold = now.AddDays(-domMax);
            predicate = predicate.And(l => (l.PublishedAt ?? l.FirstSeenAt) >= threshold);
        }

        if (filter.PriceDropsMin is { } dropsMin)
        {
            predicate = predicate.And(l => l.PriceDropCount >= dropsMin);
        }

        if (filter.CumulativeDropPercentMin is { } cumulativeMin)
        {
            predicate = predicate.And(l => l.InitialPriceAmount != null && l.PriceAmount != null && l.InitialPriceAmount > 0
                && (l.InitialPriceAmount - l.PriceAmount) / l.InitialPriceAmount * 100m >= cumulativeMin);
        }

        if (filter.NewWithinHours is { } hours)
        {
            var since = now.AddHours(-hours);
            predicate = predicate.And(l => l.FirstSeenAt >= since);
        }

        if (filter.ExcludePossession.Count > 0)
        {
            var excluded = filter.ExcludePossession.ToList();
            predicate = predicate.And(l => !excluded.Contains(l.Possession));
        }

        if (filter.Elevator is { } elevator)
        {
            predicate = predicate.And(l => l.HasElevator == elevator);
        }

        if (filter.Terrace is { } terrace)
        {
            predicate = predicate.And(l => l.HasTerrace == terrace);
        }

        if (filter.Parking is { } parking)
        {
            predicate = predicate.And(l => l.HasParking == parking);
        }

        if (!filter.IncludeNewBuild)
        {
            predicate = predicate.And(l => !l.IsNewBuild);
        }

        if (filter.MinScore is { } minScore)
        {
            predicate = predicate.And(l => l.Score != null && l.Score >= minScore);
        }

        if (filter.RenovationLevels.Count > 0)
        {
            var levels = filter.RenovationLevels.Cast<RenovationLevel?>().ToList();
            predicate = predicate.And(l => levels.Contains(l.RenovationLevel));
        }

        if (filter.ExcludeRedFlags.Count > 0)
        {
            var excluded = filter.ExcludeRedFlags.ToList();
            predicate = predicate.And(l => !l.RedFlagCodes.Any(code => excluded.Contains(code)));
        }

        if (affordability is not null)
        {
            predicate = predicate.And(Affordable(affordability));
        }

        return predicate;
    }

    private static Expression<Func<Listing, bool>> Affordable(AffordablePriceLimits limits)
    {
        var listed = limits.Subdivisions.Where(subdivision => !subdivision.IsDefault).SelectMany(subdivision => subdivision.MunicipalityKeys).Distinct().ToList();
        var anyMatch = PredicateComposer.False<Listing>();
        foreach (var subdivision in limits.Subdivisions)
        {
            var resaleMaximum = subdivision.ResaleMaximum ?? -1m;
            var newBuildMaximum = subdivision.NewBuildMaximum ?? -1m;
            Expression<Func<Listing, bool>> priceWithin = l => l.PriceAmount != null && (l.IsNewBuild ? l.PriceAmount <= newBuildMaximum : l.PriceAmount <= resaleMaximum);
            Expression<Func<Listing, bool>> located;
            if (subdivision.IsDefault)
            {
                located = l => l.Municipality == null || !listed.Contains(l.Municipality.ToLower());
            }
            else
            {
                var keys = subdivision.MunicipalityKeys.ToList();
                located = l => l.Municipality != null && keys.Contains(l.Municipality.ToLower());
            }

            anyMatch = anyMatch.Or(located.And(priceWithin));
        }

        return anyMatch;
    }

    private static string Normalize(string municipality) => municipality.Trim().ToLowerInvariant();
}
