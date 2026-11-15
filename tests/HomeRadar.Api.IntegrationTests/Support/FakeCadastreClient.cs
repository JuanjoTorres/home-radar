using HomeRadar.Application.Ports;
using HomeRadar.Domain.Intelligence;

namespace HomeRadar.Api.IntegrationTests.Support;

public sealed class FakeCadastreClient : ICadastreClient
{
    public Dictionary<string, CadastreParcelResult> Parcels { get; } = new(StringComparer.Ordinal);

    public string? ParcelAtPoint { get; set; }

    public int Calls { get; private set; }

    public Task<CadastreParcelResult> GetParcelAsync(string reference, CancellationToken cancellationToken)
    {
        Calls++;
        var parcel = reference.Length >= 14 ? reference[..14] : reference;
        return Task.FromResult(Parcels.TryGetValue(parcel, out var result) ? result : new CadastreParcelResult(parcel, CadastralLookupStatus.NotFound, [], null, null));
    }

    public Task<string?> FindParcelReferenceAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        Calls++;
        return Task.FromResult(ParcelAtPoint);
    }
}
