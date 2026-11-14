using HomeRadar.Sources.ContractTests.Support;
using HomeRadar.Sources.Solvia;

namespace HomeRadar.Sources.ContractTests;

public sealed class SolviaParserTests
{
    private static readonly Uri BaseUrl = new("https://www.solvia.es/es/comprar/viviendas/balears-illes");

    [Fact]
    public void List_page_yields_twenty_cards_with_prices_and_ids()
    {
        var html = Fixtures.Read("solvia", "list-balears.html");

        var cards = SolviaListParser.ParseCards(html, BaseUrl);

        cards.Count.ShouldBe(20);
        SolviaListParser.ParseTotal(html).ShouldBe(101);
        cards.Select(card => card.ExternalId).Distinct().Count().ShouldBe(20);
        cards.ShouldAllBe(card => card.Price != null && card.Price.Value.Amount > 1000);
        cards.ShouldAllBe(card => card.Url.AbsolutePath.StartsWith("/es/propiedades/comprar/", StringComparison.Ordinal));
        var first = cards[0];
        first.ExternalId.ShouldBe("38415-48244");
        first.Municipality.ShouldBe("Sant Josep de sa Talaia");
        first.BuiltSquareMetres.ShouldBe(285.7m);
        first.Rooms.ShouldBe(3);
        first.Bathrooms.ShouldBe(3);
        first.Condition.ShouldBe("A estrenar");
        first.PreviousPrice!.Value.Amount.ShouldBe(1_590_000m);
        first.Price!.Value.Amount.ShouldBe(1_495_000m);
        first.PhotoUrl.ShouldNotBeNull();
        cards.Count(card => card.Badges.Contains("EN SITUACIÓN ESPECIAL")).ShouldBe(2);
    }

    [Fact]
    public void Detail_page_exposes_full_property_record()
    {
        var detail = SolviaDetailParser.Parse(Fixtures.Read("solvia", "detail.html"));

        detail.Id.ShouldBe("48244-38415-O");
        detail.Price.ShouldBe(1_495_000m);
        detail.PreviousPrice.ShouldBe(1_590_000m);
        detail.SquareMetres.ShouldBe(285.7m);
        detail.Rooms.ShouldBe(3);
        detail.Characteristics!.CadastralReference.ShouldBe("9044402CD4194N0010LZ");
        detail.Characteristics.Condition.ShouldBe("Obra Nueva");
        detail.Municipality!.Name.ShouldBe("Sant Josep de sa Talaia");
        detail.Description.ShouldNotBeNullOrWhiteSpace();
        detail.Images.Count.ShouldBeGreaterThan(5);
        SolviaDetailParser.ParseSpanishTimestamp(detail.FirstPublishedAt).ShouldBe(new DateTimeOffset(2025, 10, 10, 15, 10, 50, TimeSpan.Zero));
        SolviaDetailParser.ParseSpanishTimestamp(detail.UpdatedAt)!.Value.Year.ShouldBe(2026);
    }

    [Fact]
    public void Unrecognised_detail_page_is_reported_as_schema_change() =>
        Should.Throw<Application.Ports.SourceSchemaChangedException>(() => SolviaDetailParser.Parse("<html><body>nothing</body></html>"));
}
