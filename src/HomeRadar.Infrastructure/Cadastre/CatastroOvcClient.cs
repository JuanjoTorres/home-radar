using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using HomeRadar.Application.Ports;
using HomeRadar.Domain.Intelligence;
using HomeRadar.Infrastructure.Http;

namespace HomeRadar.Infrastructure.Cadastre;

public sealed partial class CatastroOvcClient(IHttpClientFactory factory) : ICadastreClient
{
    public const string Host = "https://ovc.catastro.meh.es";
    public const string NotFoundCode = "5";
    public const string NoParcelAtPointCode = "16";

    public async Task<CadastreParcelResult> GetParcelAsync(string reference, CancellationToken cancellationToken)
    {
        var parcel = reference.Trim().ToUpperInvariant();
        parcel = parcel.Length >= 14 ? parcel[..14] : parcel;
        var uri = new Uri($"{Host}/OVCServWeb/OVCWcfCallejero/COVCCallejero.svc/json/Consulta_DNPRC?RefCat={Uri.EscapeDataString(parcel)}");
        using var client = factory.CreateClient(SourceHttpServiceCollectionExtensions.ClientName);
        var json = await client.GetStringAsync(uri, cancellationToken);
        return ParseParcel(parcel, json);
    }

    public async Task<string?> FindParcelReferenceAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        var x = longitude.ToString("0.000000", CultureInfo.InvariantCulture);
        var y = latitude.ToString("0.000000", CultureInfo.InvariantCulture);
        var uri = new Uri($"{Host}/OVCServWeb/OVCWcfCallejero/COVCCoordenadas.svc/json/Consulta_RCCOOR?CoorX={x}&CoorY={y}&SRS=EPSG:4326");
        using var client = factory.CreateClient(SourceHttpServiceCollectionExtensions.ClientName);
        var json = await client.GetStringAsync(uri, cancellationToken);
        return ParseCoordinates(json);
    }

    public static CadastreParcelResult ParseParcel(string parcelReference, string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = Property(document.RootElement, "consulta_dnprcResult") ?? document.RootElement;
        if (Errors(root).Any(code => code == NotFoundCode))
        {
            return new CadastreParcelResult(parcelReference, CadastralLookupStatus.NotFound, [], null, null);
        }

        if (Errors(root).FirstOrDefault() is { } error)
        {
            throw new InvalidOperationException($"Catastro error {error} for {parcelReference}");
        }

        var units = new List<CadastralUnit>();
        string? address = null;
        bool? rustic = IsRusticReference(parcelReference) ? true : null;
        if (Property(root, "bico") is { } single)
        {
            var property = Property(single, "bi");
            address = String(Property(single, "finca"), "ldt") ?? String(property, "ldt");
            if (String(Property(property, "idbi"), "cn") is { } kind)
            {
                rustic = string.Equals(kind, "RU", StringComparison.OrdinalIgnoreCase);
            }

            if (property is { } bi && Unit(Property(Property(bi, "idbi"), "rc"), Property(bi, "debi"), Property(Property(Property(Property(bi, "dt"), "locs"), "lous"), "lourb"), String(bi, "ldt")) is { } unit)
            {
                units.Add(unit);
            }
        }
        else
        {
            foreach (var item in Items(Property(Property(root, "lrcdnp"), "rcdnp")))
            {
                var location = Property(Property(Property(Property(item, "dt"), "locs"), "lous"), "lourb");
                if (Unit(Property(item, "rc"), Property(item, "debi"), location, null) is { } unit)
                {
                    units.Add(unit);
                }
            }
        }

        return new CadastreParcelResult(parcelReference, units.Count == 0 ? CadastralLookupStatus.NotFound : CadastralLookupStatus.Found, units, address, rustic);
    }

    public static string? ParseCoordinates(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = Property(document.RootElement, "Consulta_RCCOORResult") ?? document.RootElement;
        var errors = Errors(root).ToList();
        if (errors.Contains(NoParcelAtPointCode))
        {
            return null;
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException($"Catastro coordinates error {errors[0]}");
        }

        var first = Items(Property(Property(root, "coordenadas"), "coord")).Select(item => Property(item, "pc")).FirstOrDefault(pc => pc is not null);
        return first is { } pc && String(pc, "pc1") is { } pc1 && String(pc, "pc2") is { } pc2 ? pc1 + pc2 : null;
    }

    public static bool IsRusticReference(string reference) => RusticPattern().IsMatch(reference);

    private static CadastralUnit? Unit(JsonElement? rc, JsonElement? debi, JsonElement? location, string? address)
    {
        if (rc is not { } parts || String(parts, "pc1") is not { } pc1 || String(parts, "pc2") is not { } pc2)
        {
            return null;
        }

        var reference = pc1 + pc2 + (String(parts, "car") ?? string.Empty) + (String(parts, "cc1") ?? string.Empty) + (String(parts, "cc2") ?? string.Empty);
        var inner = Property(location, "loint");
        return new CadastralUnit(
            reference,
            String(debi, "luso"),
            Area(String(debi, "sfc")),
            int.TryParse(String(debi, "ant"), NumberStyles.None, CultureInfo.InvariantCulture, out var year) && year > 0 ? year : null,
            String(inner, "pt"),
            String(inner, "pu"),
            address);
    }

    private static decimal? Area(string? text) =>
        text is not null && decimal.TryParse(text.Replace(".", string.Empty, StringComparison.Ordinal).Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var value) && value > 0m ? value : null;

    private static IEnumerable<string> Errors(JsonElement root) =>
        Items(Property(Property(root, "lerr"), "err")).Concat(Items(Property(root, "lerr")))
            .Select(error => String(error, "cod"))
            .OfType<string>();

    private static List<JsonElement> Items(JsonElement? element) => element switch
    {
        { ValueKind: JsonValueKind.Array } array => array.EnumerateArray().ToList(),
        { ValueKind: JsonValueKind.Object } single => [single],
        _ => [],
    };

    private static JsonElement? Property(JsonElement? element, string name) =>
        element is { ValueKind: JsonValueKind.Object } value && value.TryGetProperty(name, out var property) && property.ValueKind != JsonValueKind.Null ? property : null;

    private static string? String(JsonElement? element, string name) => Property(element, name) switch
    {
        { ValueKind: JsonValueKind.String } text => text.GetString() is { Length: > 0 } value ? value.Trim() : null,
        { ValueKind: JsonValueKind.Number } number => number.GetRawText(),
        _ => null,
    };

    [GeneratedRegex("^[0-9]{5}[A-Z][0-9]{8}")]
    private static partial Regex RusticPattern();
}
