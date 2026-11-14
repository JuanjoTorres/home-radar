using HomeRadar.Domain.ValueObjects;
using NetTopologySuite.Geometries;

namespace HomeRadar.Domain.Listings;

public sealed record ListingObservation
{
    public required string ExternalId { get; init; }

    public required Uri Url { get; init; }

    public required string Title { get; init; }

    public Money? Price { get; init; }

    public Area? BuiltArea { get; init; }

    public Area? UsableArea { get; init; }

    public int? Rooms { get; init; }

    public int? Bathrooms { get; init; }

    public PropertyType PropertyType { get; init; } = PropertyType.Other;

    public string? Municipality { get; init; }

    public string? Zone { get; init; }

    public Point? Location { get; init; }

    public string? Description { get; init; }

    public IReadOnlyDictionary<string, string> Features { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    public IReadOnlyList<Uri> PhotoUrls { get; init; } = [];

    public ListingStatus Status { get; init; } = ListingStatus.Active;

    public PossessionStatus Possession { get; init; } = PossessionStatus.Unknown;

    public CadastralReference? CadastralReference { get; init; }

    public DateTimeOffset? PublishedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public Guid? AgencyId { get; init; }

    public ContentHash ComputeContentHash()
    {
        var features = string.Join(";", Features.OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => $"{pair.Key}={pair.Value}"));
        var photos = string.Join(";", PhotoUrls.Select(url => url.ToString()));
        var canonical = string.Join(
            "\n",
            ExternalId,
            Title,
            Price?.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            BuiltArea?.SquareMetres.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            UsableArea?.SquareMetres.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            Rooms?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            Bathrooms?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            PropertyType.ToString(),
            Municipality ?? string.Empty,
            Zone ?? string.Empty,
            Description ?? string.Empty,
            features,
            photos,
            Status.ToString(),
            Possession.ToString(),
            CadastralReference?.Value ?? string.Empty);
        return ContentHash.Of(canonical);
    }
}
