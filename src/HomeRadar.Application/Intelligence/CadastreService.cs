using HomeRadar.Application.Ports;
using HomeRadar.Domain.Intelligence;
using HomeRadar.Domain.Listings;
using Microsoft.EntityFrameworkCore;

namespace HomeRadar.Application.Intelligence;

public sealed class CadastreService(IHomeRadarDbContext db, ICadastreClient client, TimeProvider clock)
{
    public static readonly TimeSpan MaximumAge = TimeSpan.FromDays(180);
    public const decimal UnitMatchTolerancePercent = 20m;

    public async Task<CadastralFacts?> ResolveAsync(Listing listing, CancellationToken cancellationToken)
    {
        var parcelReference = listing.CadastralReference?.ParcelPart ?? listing.CadastralParcelReference;
        if (parcelReference is null && listing.Location is { } point)
        {
            parcelReference = await client.FindParcelReferenceAsync(point.Y, point.X, cancellationToken);
        }

        if (parcelReference is null)
        {
            return null;
        }

        var parcel = await ParcelAsync(parcelReference, cancellationToken);
        return CadastralMatcher.Match(parcel, listing.CadastralReference?.Value, listing.BuiltSquareMetres, UnitMatchTolerancePercent);
    }

    public async Task<CadastralParcel> ParcelAsync(string parcelReference, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var cached = await db.CadastralParcels.SingleOrDefaultAsync(parcel => parcel.ParcelReference == parcelReference, cancellationToken)
            ?? db.CadastralParcels.Local.FirstOrDefault(parcel => parcel.ParcelReference == parcelReference);
        if (cached is not null && !cached.IsStale(now, MaximumAge))
        {
            return cached;
        }

        var fetched = await client.GetParcelAsync(parcelReference, cancellationToken);
        if (cached is null)
        {
            cached = new CadastralParcel(parcelReference, fetched.Status, fetched.Units, fetched.Address, fetched.IsRustic, now);
            db.CadastralParcels.Add(cached);
        }
        else
        {
            cached.Refresh(fetched.Status, fetched.Units, fetched.Address, fetched.IsRustic, now);
        }

        return cached;
    }
}
