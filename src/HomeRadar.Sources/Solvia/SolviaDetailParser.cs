using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using HomeRadar.Application.Ports;

namespace HomeRadar.Sources.Solvia;

public sealed record SolviaPropertyDetail
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("idVivienda")]
    public string? IdVivienda { get; init; }

    [JsonPropertyName("idPromocion")]
    public string? IdPromocion { get; init; }

    [JsonPropertyName("textoDescripcion")]
    public string? Description { get; init; }

    [JsonPropertyName("tituloFicha")]
    public string? Title { get; init; }

    [JsonPropertyName("enSituacionEspecial")]
    public int SpecialSituation { get; init; }

    [JsonPropertyName("caracteristicas")]
    public SolviaCharacteristics? Characteristics { get; init; }

    [JsonPropertyName("cp")]
    public string? PostalCode { get; init; }

    [JsonPropertyName("reservado")]
    public string? Reserved { get; init; }

    [JsonPropertyName("reformar")]
    public string? ToReform { get; init; }

    [JsonPropertyName("direccion")]
    public string? Address { get; init; }

    [JsonPropertyName("m2")]
    public decimal? SquareMetres { get; init; }

    [JsonPropertyName("poblacion")]
    public SolviaNamed? Municipality { get; init; }

    [JsonPropertyName("precio")]
    public decimal? Price { get; init; }

    [JsonPropertyName("precioAntes")]
    public decimal? PreviousPrice { get; init; }

    [JsonPropertyName("tipoVivienda")]
    public SolviaNamed? PropertyType { get; init; }

    [JsonPropertyName("totalBanyos")]
    public int? Bathrooms { get; init; }

    [JsonPropertyName("totalDormitorios")]
    public int? Rooms { get; init; }

    [JsonPropertyName("listaImagenesInmueble")]
    public IReadOnlyList<SolviaImage> Images { get; init; } = [];

    [JsonPropertyName("fichaFechaActualizacionProducto")]
    public string? UpdatedAt { get; init; }

    [JsonPropertyName("fichaFechaPrimeraPub")]
    public string? FirstPublishedAt { get; init; }

    [JsonPropertyName("fichaOrigenProducto")]
    public string? Origin { get; init; }

    [JsonPropertyName("campanya")]
    public SolviaCampaign? Campaign { get; init; }
}

public sealed record SolviaCharacteristics
{
    [JsonPropertyName("refCatastral")]
    public string? CadastralReference { get; init; }

    [JsonPropertyName("estado")]
    public string? Condition { get; init; }

    [JsonPropertyName("piscina")]
    public bool Pool { get; init; }

    [JsonPropertyName("garaje")]
    public bool Garage { get; init; }

    [JsonPropertyName("trastero")]
    public bool StorageRoom { get; init; }

    [JsonPropertyName("reservado")]
    public bool Reserved { get; init; }

    [JsonPropertyName("vpo")]
    public bool Vpo { get; init; }

    [JsonPropertyName("importeIbi")]
    public decimal? Ibi { get; init; }

    [JsonPropertyName("importeGastosComunidad")]
    public decimal? CommunityFees { get; init; }
}

public sealed record SolviaNamed([property: JsonPropertyName("id")] JsonElement Id, [property: JsonPropertyName("name")] string? Name);

public sealed record SolviaImage([property: JsonPropertyName("url")] string? Url);

public sealed record SolviaCampaign([property: JsonPropertyName("name")] string? Name);

public static partial class SolviaDetailParser
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = false, NumberHandling = JsonNumberHandling.AllowReadingFromString };

    public static SolviaPropertyDetail Parse(string html)
    {
        var match = NgState().Match(html);
        if (!match.Success)
        {
            throw new SourceSchemaChangedException("solvia", "detail page has no ng-state transfer block");
        }

        using var document = JsonDocument.Parse(match.Groups[1].Value);
        if (!document.RootElement.TryGetProperty("propertyBasicDetail", out var detail))
        {
            throw new SourceSchemaChangedException("solvia", "ng-state has no propertyBasicDetail");
        }

        return detail.Deserialize<SolviaPropertyDetail>(Options) ?? throw new SourceSchemaChangedException("solvia", "propertyBasicDetail could not be deserialized");
    }

    public static DateTimeOffset? ParseSpanishTimestamp(string? value) =>
        DateTimeOffset.TryParseExact(value, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out var parsed) ? parsed : null;

    [GeneratedRegex("<script id=\"ng-state\" type=\"application/json\">(.*?)</script>", RegexOptions.Singleline)]
    private static partial Regex NgState();
}
