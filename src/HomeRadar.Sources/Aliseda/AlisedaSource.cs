using System.Runtime.CompilerServices;
using System.Text.Json;
using HomeRadar.Application.Ports;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.ValueObjects;
using HomeRadar.Sources.Common;
using NetTopologySuite.Geometries;

namespace HomeRadar.Sources.Aliseda;

public sealed record AlisedaConfiguration(
    string ListUrl = "https://www.alisedainmobiliaria.com/comprar-viviendas/islas-baleares",
    int MaxPages = 50);

/// <summary>
/// Aliseda's listing pages embed the full search API response (every property with description, coordinates,
/// cadastral reference and possession status) in the Angular transfer state, so no detail pages are needed.
/// </summary>
public sealed class AlisedaSource(ISourceHttpClient http) : IListingSource
{
    public const string Key = "aliseda";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string AdapterKey => Key;

    public async IAsyncEnumerable<FetchedListing> FetchAsync(SourceFetchContext context, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var configuration = JsonSerializer.Deserialize<AlisedaConfiguration>(context.Source.ConfigurationJson, JsonOptions) ?? new AlisedaConfiguration();
        var baseUrl = new Uri(configuration.ListUrl);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var page = 1;
        int lastPage;
        do
        {
            var url = new Uri($"{configuration.ListUrl}?page={page}");
            var fetched = await http.GetAsync(url, cancellationToken);
            var search = AlisedaParser.ParseSearchPage(fetched.Body);
            lastPage = Math.Min(search.LastPage, configuration.MaxPages);
            foreach (var property in search.Data)
            {
                if (property.Active == 0 || !seen.Add(property.Id))
                {
                    continue;
                }

                var raw = new RawDocument("application/json", JsonSerializer.Serialize(property, JsonOptions));
                yield return FetchedListing.Of(ToObservation(property, baseUrl), raw);
            }

            page++;
            if (!context.FullScan)
            {
                yield break;
            }
        }
        while (page <= lastPage);
    }

    private static ListingObservation ToObservation(AlisedaProperty property, Uri baseUrl)
    {
        var features = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [FeatureKeys.Elevator] = Flag(property.Elevator),
            [FeatureKeys.Terrace] = Flag(property.Terrace),
            [FeatureKeys.Parking] = Flag(property.Garage),
            [FeatureKeys.Pool] = Flag(property.Pool + property.CommunityPool),
            [FeatureKeys.StorageRoom] = Flag(property.StorageRoom),
            [FeatureKeys.NewBuild] = Flag(property.NewBuild),
        };
        AddIf(features, FeatureKeys.EnergyRating, property.EnergyRating);
        AddIf(features, FeatureKeys.PostalCode, property.Address?.PostalCode);
        AddIf(features, FeatureKeys.Address, Parsing.Collapse($"{property.Address?.StreetType} {property.Address?.StreetName} {property.Address?.StreetNumber}".Replace(" 0", string.Empty, StringComparison.Ordinal)));
        AddIf(features, FeatureKeys.Floor, property.Address?.Floor);
        AddIf(features, FeatureKeys.Reference, property.OriginalId);
        AddIf(features, FeatureKeys.Condition, property.Channel);
        AddIf(features, FeatureKeys.PreviousPrice, property.Operation?.PreviousPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture));
        AddIf(features, FeatureKeys.Discount, property.Operation?.DiscountPercent?.ToString(System.Globalization.CultureInfo.InvariantCulture));
        AddIf(features, FeatureKeys.PlotSquareMetres, property.PlotArea is > 0 ? property.PlotArea.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : null);
        AddIf(features, FeatureKeys.Campaign, string.Join(" | ", property.Campaigns.Select(campaign => campaign.Name).Where(name => !string.IsNullOrWhiteSpace(name))));
        AddIf(features, FeatureKeys.SpecialSituation, property.SpecialSituation);
        AddIf(features, "possessionLabel", property.Possession);
        AddIf(features, "rentalContractEnd", property.RentalContractEnd);
        if (property.Proindiviso != 0)
        {
            features["proindiviso"] = "true";
        }

        Point? location = property.Address is { Latitude: { } lat, Longitude: { } lon } && lat != 0 && lon != 0
            ? new Point(lon, lat) { SRID = UtmProjection.Wgs84Srid }
            : null;
        var photos = property.Images
            .Where(image => !string.IsNullOrWhiteSpace(image.Uri) && !(image.Order?.StartsWith("ZZ", StringComparison.Ordinal) ?? false))
            .OrderBy(image => image.Order, StringComparer.Ordinal)
            .Select(image => new Uri(image.Uri!))
            .ToList();

        var typeLabel = property.Subtype switch
        {
            36 => "Piso",
            32 => "Casa independiente",
            31 => "Chalet adosado",
            _ => property.Type == 10 ? "Vivienda" : "Otro",
        };

        return new ListingObservation
        {
            ExternalId = property.Id,
            Url = new Uri(baseUrl, $"/inmueble/{property.Id}"),
            Title = Parsing.Collapse($"{typeLabel} en {features.GetValueOrDefault(FeatureKeys.Address)}, {property.Address?.City}"),
            Price = property.Operation?.Price is > 0 ? Money.Euros(property.Operation.Price.Value) : null,
            BuiltArea = Area.FromNullable(property.ConstructedArea),
            UsableArea = Area.FromNullable(property.UsableArea),
            Rooms = property.Bedrooms is > 0 ? property.Bedrooms : null,
            Bathrooms = property.Bathrooms is > 0 ? property.Bathrooms : null,
            PropertyType = Parsing.MapPropertyType(typeLabel),
            Municipality = property.Address?.City,
            Zone = features.GetValueOrDefault(FeatureKeys.Address),
            Location = location,
            Description = property.Description?.Replace("<br/>", "\n", StringComparison.OrdinalIgnoreCase).Trim(),
            Features = features,
            PhotoUrls = photos,
            Possession = PossessionOf(property),
            CadastralReference = CadastralReference.TryParse(property.CadastralReference, out var reference) ? reference : null,
            PublishedAt = AlisedaParser.ParseDate(property.Operation?.PublishedOn),
            UpdatedAt = AlisedaParser.ParseDate(property.LastModifiedDate),
        };
    }

    private static PossessionStatus PossessionOf(AlisedaProperty property)
    {
        if (!string.IsNullOrWhiteSpace(property.RentalContractEnd) || string.Equals(property.RentalStatus, "ALQUILADO", StringComparison.OrdinalIgnoreCase))
        {
            return PossessionStatus.RentedUntil;
        }

        return property.Possession?.ToUpperInvariant() switch
        {
            "LIBRE" => PossessionStatus.Vacant,
            "OCUPADO" or "SIN POSESION" or "SIN POSESIÓN" => PossessionStatus.NoPossession,
            null or "" => PossessionStatus.Unknown,
            _ => PossessionStatus.Occupied,
        };
    }

    private static string Flag(int value) => value > 0 ? "true" : "false";

    private static void AddIf(Dictionary<string, string> features, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            features[key] = value;
        }
    }
}
