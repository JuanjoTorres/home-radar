using HomeRadar.Application.Notifications;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Application.Tests;

public sealed class ListingMessageFormatterTests
{
    [Fact]
    public void New_listing_message_is_short_and_carries_price_per_square_metre_and_link()
    {
        var listing = Listing.Create(Guid.NewGuid(), "solvia", SellerType.Servicer, new ListingObservation
        {
            ExternalId = "1",
            Url = new Uri("https://www.solvia.es/es/propiedades/comprar/piso-inca-3-dormitorios-1-2"),
            Title = "Piso en Inca",
            Price = Money.Euros(245000),
            BuiltArea = new Area(95),
            Rooms = 3,
            Bathrooms = 2,
            PropertyType = PropertyType.Flat,
            Municipality = "Inca",
            Possession = PossessionStatus.NoPossession,
        }, DateTimeOffset.UtcNow);

        var text = ListingMessageFormatter.NewListing(listing, "Solvia", ["Inca hasta 260k"]);

        text.ShouldContain("245.000 €");
        text.ShouldContain("2.579 €/m²");
        text.ShouldContain("📍 Inca");
        text.ShouldContain("Posesión: NoPossession");
        text.ShouldContain("Alertas: Inca hasta 260k");
        text.ShouldEndWith(listing.Url.ToString());
        text.Length.ShouldBeLessThan(600);
    }
}
