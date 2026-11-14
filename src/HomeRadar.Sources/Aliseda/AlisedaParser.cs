using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using HomeRadar.Application.Ports;

namespace HomeRadar.Sources.Aliseda;

public sealed record AlisedaSearchPage(
    [property: JsonPropertyName("current_page")] int CurrentPage,
    [property: JsonPropertyName("last_page")] int LastPage,
    [property: JsonPropertyName("per_page")] int PerPage,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("data")] IReadOnlyList<AlisedaProperty> Data);

public sealed record AlisedaProperty
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("original_id")]
    public string? OriginalId { get; init; }

    [JsonPropertyName("FkTipo")]
    public int Type { get; init; }

    [JsonPropertyName("FkSubtipo")]
    public int Subtype { get; init; }

    [JsonPropertyName("ConstructedArea")]
    public decimal? ConstructedArea { get; init; }

    [JsonPropertyName("UsableArea")]
    public decimal? UsableArea { get; init; }

    [JsonPropertyName("SupParcela")]
    public decimal? PlotArea { get; init; }

    [JsonPropertyName("EnergyRating")]
    public string? EnergyRating { get; init; }

    [JsonPropertyName("RefCatastral")]
    public string? CadastralReference { get; init; }

    [JsonPropertyName("LastModifiedDate")]
    public string? LastModifiedDate { get; init; }

    [JsonPropertyName("Estrenar")]
    public int NewBuild { get; init; }

    [JsonPropertyName("Activo")]
    public int Active { get; init; }

    [JsonPropertyName("posesion")]
    public string? Possession { get; init; }

    [JsonPropertyName("situacionEspecial")]
    public string? SpecialSituation { get; init; }

    [JsonPropertyName("estadoAlquiler")]
    public string? RentalStatus { get; init; }

    [JsonPropertyName("FechaFinContratoAlquiler")]
    public string? RentalContractEnd { get; init; }

    [JsonPropertyName("Proindiviso")]
    public int Proindiviso { get; init; }

    [JsonPropertyName("redComercial")]
    public string? Channel { get; init; }

    [JsonPropertyName("Description")]
    public string? Description { get; init; }

    [JsonPropertyName("Bathrooms")]
    public int? Bathrooms { get; init; }

    [JsonPropertyName("Bedrooms")]
    public int? Bedrooms { get; init; }

    [JsonPropertyName("Ascensor")]
    public int Elevator { get; init; }

    [JsonPropertyName("Terraza")]
    public int Terrace { get; init; }

    [JsonPropertyName("Garage")]
    public int Garage { get; init; }

    [JsonPropertyName("Piscina")]
    public int Pool { get; init; }

    [JsonPropertyName("PiscinaComunitaria")]
    public int CommunityPool { get; init; }

    [JsonPropertyName("Trastero")]
    public int StorageRoom { get; init; }

    [JsonPropertyName("address")]
    public AlisedaAddress? Address { get; init; }

    [JsonPropertyName("operacion")]
    public AlisedaOperation? Operation { get; init; }

    [JsonPropertyName("imagenes")]
    public IReadOnlyList<AlisedaImage> Images { get; init; } = [];

    [JsonPropertyName("campaigns")]
    public IReadOnlyList<AlisedaCampaign> Campaigns { get; init; } = [];
}

public sealed record AlisedaAddress(
    [property: JsonPropertyName("Ciudad")] string? City,
    [property: JsonPropertyName("StreetName")] string? StreetName,
    [property: JsonPropertyName("StreetNumber")] string? StreetNumber,
    [property: JsonPropertyName("PostalCode")] string? PostalCode,
    [property: JsonPropertyName("TipoVia")] string? StreetType,
    [property: JsonPropertyName("Piso")] string? Floor,
    [property: JsonPropertyName("Latitude")] double? Latitude,
    [property: JsonPropertyName("Longitude")] double? Longitude);

public sealed record AlisedaOperation(
    [property: JsonPropertyName("FechaPublicacion")] string? PublishedOn,
    [property: JsonPropertyName("DescuentoPrecio")] decimal? DiscountPercent,
    [property: JsonPropertyName("Precio")] decimal? Price,
    [property: JsonPropertyName("PrecioAnterior")] decimal? PreviousPrice,
    [property: JsonPropertyName("Rebajado")] int Discounted);

public sealed record AlisedaImage([property: JsonPropertyName("Orden")] string? Order, [property: JsonPropertyName("Uri")] string? Uri);

public sealed record AlisedaCampaign([property: JsonPropertyName("Nombre")] string? Name);

public static partial class AlisedaParser
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = false, NumberHandling = JsonNumberHandling.AllowReadingFromString };

    public static AlisedaSearchPage ParseSearchPage(string html)
    {
        var match = ServerState().Match(html);
        if (!match.Success)
        {
            throw new SourceSchemaChangedException("aliseda", "listing page has no serverApp-state block");
        }

        using var document = JsonDocument.Parse(match.Groups[1].Value.Replace("&q;", "\"", StringComparison.Ordinal));
        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (property.Name.Contains("/api/v2/new-search?", StringComparison.Ordinal))
            {
                return property.Value.Deserialize<AlisedaSearchPage>(Options) ?? throw new SourceSchemaChangedException("aliseda", "search state could not be deserialized");
            }
        }

        throw new SourceSchemaChangedException("aliseda", "serverApp-state has no new-search entry");
    }

    public static DateTimeOffset? ParseDate(string? value) =>
        DateTimeOffset.TryParseExact(value, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out var parsed) ? parsed : null;

    [GeneratedRegex("<script id=\"serverApp-state\" type=\"application/json\">(.*?)</script>", RegexOptions.Singleline)]
    private static partial Regex ServerState();
}
