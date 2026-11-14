using HomeRadar.Domain.Listings;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Domain.Tests;

internal static class ListingObservationBuilder
{
    public static ListingObservation Sample(decimal price = 245000m, string description = "Piso luminoso en Inca", params string[] photos) =>
        new()
        {
            ExternalId = "12345",
            Url = new Uri("https://example.test/inmueble/12345"),
            Title = "Piso en Inca",
            Price = Money.Euros(price),
            BuiltArea = new Area(95),
            Rooms = 3,
            Bathrooms = 2,
            PropertyType = PropertyType.Flat,
            Municipality = "Inca",
            Description = description,
            Features = new Dictionary<string, string>(StringComparer.Ordinal) { ["elevator"] = "true" },
            PhotoUrls = photos.Length == 0
                ? [new Uri("https://example.test/p/1.jpg"), new Uri("https://example.test/p/2.jpg")]
                : photos.Select(photo => new Uri(photo)).ToList(),
        };
}
