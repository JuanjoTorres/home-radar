using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using HomeRadar.Application.Ports;
using HomeRadar.Sources.Common;

namespace HomeRadar.Sources.Inmovilla;

public sealed record InmovillaPageInfo(int Position, int Elements, int Total);

public sealed record InmovillaListing
{
    [JsonPropertyName("cod_ofer")]
    public long Code { get; init; }

    [JsonPropertyName("ref")]
    public string? Reference { get; init; }

    [JsonPropertyName("nodisponible")]
    public int Unavailable { get; init; }

    [JsonPropertyName("precioinmo")]
    public decimal? Price { get; init; }

    [JsonPropertyName("outlet")]
    public decimal? PreviousPrice { get; init; }

    [JsonPropertyName("aconsultar")]
    public int PriceOnRequest { get; init; }

    [JsonPropertyName("m_cons")]
    public decimal? BuiltSquareMetres { get; init; }

    [JsonPropertyName("m_uties")]
    public decimal? UsableSquareMetres { get; init; }

    [JsonPropertyName("m_parcela")]
    public decimal? PlotSquareMetres { get; init; }

    [JsonPropertyName("habitaciones")]
    public JsonElement RoomsRaw { get; init; }

    [JsonPropertyName("total_hab")]
    public JsonElement TotalRoomsRaw { get; init; }

    [JsonPropertyName("banyos")]
    public JsonElement BathroomsRaw { get; init; }

    [JsonIgnore]
    public int? Rooms => AsInt(RoomsRaw) ?? AsInt(TotalRoomsRaw);

    [JsonIgnore]
    public int? Bathrooms => AsInt(BathroomsRaw);

    private static int? AsInt(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Number => element.TryGetDecimal(out var number) && number > 0 ? (int)number : null,
        JsonValueKind.String => decimal.TryParse(element.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsed) && parsed > 0 ? (int)parsed : null,
        _ => null,
    };

    [JsonPropertyName("ascensor")]
    public int Elevator { get; init; }

    [JsonPropertyName("terraza")]
    public int Terrace { get; init; }

    [JsonPropertyName("plaza_gara")]
    public int Parking { get; init; }

    [JsonPropertyName("piscina_com")]
    public int CommunityPool { get; init; }

    [JsonPropertyName("piscina_prop")]
    public int PrivatePool { get; init; }

    [JsonPropertyName("latitud")]
    public double? Latitude { get; init; }

    [JsonPropertyName("altitud")]
    public double? Longitude { get; init; }

    [JsonPropertyName("numfotos")]
    public int PhotoCount { get; init; }

    [JsonPropertyName("fotoletra")]
    public int PhotoLetter { get; init; }

    [JsonPropertyName("srvfotos")]
    public int PhotoServer { get; init; }

    [JsonPropertyName("numagencia")]
    public int AgencyNumber { get; init; }

    [JsonPropertyName("fecha")]
    public string? CreatedAt { get; init; }

    [JsonPropertyName("fechacambio")]
    public string? ChangedAt { get; init; }

    [JsonPropertyName("estadoficha")]
    public int Status { get; init; }

    [JsonPropertyName("estadofichaLetrero")]
    public string? StatusLabel { get; init; }

    [JsonPropertyName("ciudad")]
    public string? City { get; init; }

    [JsonPropertyName("zona")]
    public string? Zone { get; init; }

    [JsonPropertyName("nbtipo")]
    public string? TypeLabel { get; init; }

    [JsonPropertyName("nbconservacion")]
    public string? ConditionLabel { get; init; }

    [JsonPropertyName("antiguedad")]
    public int? YearBuilt { get; init; }

    [JsonPropertyName("energialetra")]
    public string? EnergyLetter { get; init; }

    [JsonPropertyName("planta")]
    public JsonElement Floor { get; init; }

    [JsonPropertyName("cp")]
    public JsonElement PostalCode { get; init; }

    [JsonPropertyName("exclu")]
    public int Exclusive { get; init; }

    [JsonPropertyName("tipovpo")]
    public int VpoType { get; init; }

    [JsonPropertyName("tituloauto")]
    public string? AutoTitle { get; init; }

    [JsonPropertyName("tituloDefecto")]
    public string? DefaultTitle { get; init; }

    [JsonPropertyName("urlfichaauto")]
    public string? DetailPath { get; init; }

    [JsonPropertyName("keyacci")]
    public int Action { get; init; }

    [JsonIgnore]
    public string RawJson { get; init; } = "{}";
}

public static partial class InmovillaParser
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = false, NumberHandling = JsonNumberHandling.AllowReadingFromString };

    public static (InmovillaPageInfo Page, IReadOnlyList<InmovillaListing> Listings) ParseListPage(string html)
    {
        var match = ListPattern().Match(html);
        if (!match.Success)
        {
            throw new SourceSchemaChangedException("inmovilla", "listing page has no listados.paginacion block");
        }

        using var document = JsonDocument.Parse(match.Groups[1].Value);
        InmovillaPageInfo? page = null;
        var listings = new List<InmovillaListing>();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (property.Name == "0")
            {
                page = new InmovillaPageInfo(property.Value.GetProperty("posicion").GetInt32(), property.Value.GetProperty("elementos").GetInt32(), property.Value.GetProperty("total").GetInt32());
                continue;
            }

            var listing = property.Value.Clone().Deserialize<InmovillaListing>(Options);
            if (listing is not null)
            {
                listings.Add(listing with { RawJson = property.Value.GetRawText() });
            }
        }

        return (page ?? throw new SourceSchemaChangedException("inmovilla", "pagination metadata missing"), listings);
    }

    public static InmovillaListing? ParseDetailRecord(string html)
    {
        var match = DetailPattern().Match(html);
        if (!match.Success)
        {
            return null;
        }

        using var document = JsonDocument.Parse(match.Groups[1].Value);
        return document.RootElement.EnumerateArray().Skip(1).FirstOrDefault().Clone().Deserialize<InmovillaListing>(Options);
    }

    public static string? ParseDescription(string html)
    {
        var document = HtmlParsing.Parse(html);
        var block = document.QuerySelectorAll("[id*='descripcion'], [class*='descripcion']")
            .Select(element => element.Text())
            .FirstOrDefault(text => text.Length > 40);
        return block ?? document.QuerySelector("meta[name='description']")?.GetAttribute("content")?.Trim();
    }

    public static DateTimeOffset? ParseTimestamp(string? value) =>
        DateTimeOffset.TryParseExact(value, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out var parsed) && parsed.Year > 1900 ? parsed : null;

    [GeneratedRegex(@"listados\.paginacion\s*=\s*(\{.*?\});\s*\n", RegexOptions.Singleline)]
    private static partial Regex ListPattern();

    [GeneratedRegex(@"listados\.fichapropiedad\s*=\s*(\[.*?\]);\s*\n", RegexOptions.Singleline)]
    private static partial Regex DetailPattern();
}
