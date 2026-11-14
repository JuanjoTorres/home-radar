using HomeRadar.Sources.ContractTests.Support;
using HomeRadar.Sources.Diglo;

namespace HomeRadar.Sources.ContractTests;

public sealed class DigloParserTests
{
    private static readonly Uri BaseUrl = new("https://digloservicer.com/venta-activos/cualquiera/islas-baleares");

    [Fact]
    public void List_page_yields_six_cards_and_total_rows()
    {
        var html = Fixtures.Read("diglo", "list-balears.html");

        var cards = DigloParser.ParseCards(html, BaseUrl);

        cards.Count.ShouldBe(6);
        DigloParser.ParseTotalRows(html).ShouldBe(14);
        var first = cards[0];
        first.ExternalId.ShouldBe("RET0000206481");
        first.Municipality.ShouldBe("Manacor");
        first.Price!.Value.Amount.ShouldBe(295_000m);
        first.SquareMetres.ShouldBe(122m);
        first.ProductType.ShouldBe("Local");
        first.PhotoUrls.Count.ShouldBe(3);
    }

    [Fact]
    public void Detail_page_has_description_coordinates_and_photos()
    {
        var detail = DigloParser.ParseDetail(Fixtures.Read("diglo", "detail.html"), BaseUrl, "RET0000206481");

        detail.Description.ShouldStartWith("Local Comercial en la localidad de Manacor");
        detail.Latitude!.Value.ShouldBe(39.540443668877, 1e-9);
        detail.Longitude!.Value.ShouldBe(3.332950865447, 1e-9);
        detail.PhotoUrls.Count.ShouldBe(3);
        detail.Price!.Value.Amount.ShouldBe(295_000m);
    }
}
