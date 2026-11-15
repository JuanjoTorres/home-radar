using HomeRadar.Application.Notifications;
using HomeRadar.Domain.Intelligence;
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
        text.ShouldNotContain("📊");
    }

    [Fact]
    public void Message_shows_score_renovation_and_red_flags()
    {
        var listing = Listing.Create(Guid.NewGuid(), "solvia", SellerType.Servicer, new ListingObservation
        {
            ExternalId = "2",
            Url = new Uri("https://example.test/2"),
            Title = "Piso en Manacor",
            Price = Money.Euros(150000),
            BuiltArea = new Area(80),
            PropertyType = PropertyType.Flat,
            Municipality = "Manacor",
        }, DateTimeOffset.UtcNow);
        listing.ApplyIntelligence(RenovationLevel.Full, 90_728m, false, 27, ["occupied", "unknownCode"], DateTimeOffset.UtcNow).ShouldBeTrue();
        listing.ApplyIntelligence(RenovationLevel.Full, 90_728m, false, 27, ["unknownCode", "occupied"], DateTimeOffset.UtcNow).ShouldBeFalse();

        var text = ListingMessageFormatter.NewListing(listing, "Solvia", []);

        text.ShouldContain("📊 Puntuación 27/100");
        text.ShouldContain("🛠️ Reforma integral ~90.728 €");
        text.ShouldContain("🚩 Ocupada · unknownCode");
    }
}
