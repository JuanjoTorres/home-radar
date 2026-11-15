using HomeRadar.Domain.Common;

namespace HomeRadar.Domain.Intelligence;

public enum CadastralLookupStatus
{
    Found = 0,
    NotFound = 1,
}

public sealed record CadastralUnit(string Reference, string? Use, decimal? BuiltSquareMetres, int? YearBuilt, string? Floor, string? Door, string? Address);

public sealed class CadastralParcel : Entity
{
    private List<CadastralUnit> _units = [];

    private CadastralParcel()
    {
    }

    public CadastralParcel(string parcelReference, CadastralLookupStatus status, IEnumerable<CadastralUnit> units, string? address, bool? isRustic, DateTimeOffset fetchedAt)
    {
        ParcelReference = parcelReference;
        Refresh(status, units, address, isRustic, fetchedAt);
    }

    public string ParcelReference { get; private init; } = string.Empty;

    public CadastralLookupStatus Status { get; private set; }

    public IReadOnlyList<CadastralUnit> Units => _units;

    public string? Address { get; private set; }

    public bool? IsRustic { get; private set; }

    public DateTimeOffset FetchedAt { get; private set; }

    public void Refresh(CadastralLookupStatus status, IEnumerable<CadastralUnit> units, string? address, bool? isRustic, DateTimeOffset fetchedAt)
    {
        Status = status;
        _units = units.ToList();
        Address = address;
        IsRustic = isRustic;
        FetchedAt = fetchedAt;
    }

    public bool IsStale(DateTimeOffset now, TimeSpan maximumAge) => now - FetchedAt > maximumAge;
}
