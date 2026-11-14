namespace HomeRadar.Api.Contracts;

/// <summary>Monetary amount. Always two fraction digits.</summary>
/// <param name="Amount">Amount in the currency, e.g. 245000.00.</param>
/// <param name="Currency">ISO 4217 code; only <c>EUR</c> is used.</param>
public sealed record MoneyDto(decimal Amount, string Currency = "EUR")
{
    public static MoneyDto? From(Domain.ValueObjects.Money? money) => money is { } value ? new MoneyDto(value.Amount, value.Currency.ToString()) : null;

    public static MoneyDto From(Domain.ValueObjects.Money money) => new(money.Amount, money.Currency.ToString());
}

/// <summary>GeoJSON point (WGS84).</summary>
/// <param name="Type">Always <c>Point</c>.</param>
/// <param name="Coordinates">Longitude then latitude, e.g. [2.9108, 39.7211].</param>
public sealed record GeoPointDto(string Type, double[] Coordinates)
{
    public static GeoPointDto? From(NetTopologySuite.Geometries.Point? point) => point is null ? null : new GeoPointDto("Point", [point.X, point.Y]);
}

/// <summary>Cursor-paginated envelope shared by every list operation.</summary>
/// <typeparam name="T">Item type.</typeparam>
/// <param name="Items">Items of this page, in the requested order.</param>
/// <param name="NextCursor">Opaque cursor for the next page; <c>null</c> when there is none.</param>
/// <param name="HasMore">Whether another page exists.</param>
public sealed record PageDto<T>(IReadOnlyList<T> Items, string? NextCursor, bool HasMore);

/// <summary>Range filter with inclusive bounds.</summary>
/// <param name="Min">Lower bound, inclusive.</param>
/// <param name="Max">Upper bound, inclusive.</param>
public sealed record DecimalRangeDto(decimal? Min, decimal? Max);

/// <summary>Integer range filter with inclusive bounds.</summary>
/// <param name="Min">Lower bound, inclusive.</param>
/// <param name="Max">Upper bound, inclusive.</param>
public sealed record IntRangeDto(int? Min, int? Max);

/// <summary>Circle around a point.</summary>
/// <param name="Latitude">WGS84 latitude, e.g. 39.7211.</param>
/// <param name="Longitude">WGS84 longitude, e.g. 2.9108.</param>
/// <param name="Metres">Radius in metres (50–50000).</param>
public sealed record RadiusDto(double Latitude, double Longitude, int Metres);
