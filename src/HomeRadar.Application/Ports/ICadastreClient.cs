using HomeRadar.Domain.Intelligence;

namespace HomeRadar.Application.Ports;

/// <summary>Read-only access to the Catastro's free OVC web services (non-protected data only: no owners, no cadastral value).</summary>
public interface ICadastreClient
{
    /// <summary>Gets the parcel and its units for a 14- or 20-character cadastral reference.</summary>
    /// <param name="reference">Cadastral reference; a 20-character one is reduced to its parcel part.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The parcel with every unit the Catastro lists, or a <see cref="CadastralLookupStatus.NotFound"/> result.</returns>
    Task<CadastreParcelResult> GetParcelAsync(string reference, CancellationToken cancellationToken);

    /// <summary>Finds the 14-character parcel reference at a WGS84 point.</summary>
    /// <param name="latitude">Latitude in degrees.</param>
    /// <param name="longitude">Longitude in degrees.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The parcel reference, or null when the point is not inside any parcel.</returns>
    Task<string?> FindParcelReferenceAsync(double latitude, double longitude, CancellationToken cancellationToken);
}

/// <summary>Parcel data returned by the Catastro.</summary>
/// <param name="ParcelReference">14-character parcel reference.</param>
/// <param name="Status">Whether the Catastro knows the parcel.</param>
/// <param name="Units">Units (flats, garages, premises…) with use, built area and year.</param>
/// <param name="Address">Literal address of the parcel.</param>
/// <param name="IsRustic">True for rustic land, when the Catastro says so.</param>
public sealed record CadastreParcelResult(string ParcelReference, CadastralLookupStatus Status, IReadOnlyList<CadastralUnit> Units, string? Address, bool? IsRustic);
