using HomeRadar.Sources.ContractTests.Support;
using HomeRadar.Sources.Inmovilla;

namespace HomeRadar.Sources.ContractTests;

public sealed class InmovillaParserTests
{
    [Fact]
    public void Sale_list_page_embeds_listings_and_pagination()
    {
        var (page, listings) = InmovillaParser.ParseListPage(Fixtures.Read("inmovilla-native", "mir-venta.html"));

        page.ShouldBe(new InmovillaPageInfo(1, 12, 29));
        listings.Count.ShouldBe(12);
        var first = listings[0];
        first.Code.ShouldBe(4446305);
        first.Reference.ShouldBe("S10007");
        first.Price.ShouldBe(47_000m);
        first.PreviousPrice.ShouldBe(53_000m);
        first.City.ShouldBe("Llucmajor");
        first.Latitude!.Value.ShouldBe(39.49066788319245, 1e-9);
        first.PhotoCount.ShouldBe(16);
        first.AgencyNumber.ShouldBe(4445);
        first.DetailPath.ShouldBe("ficha/terreno-rural/llucmajor/llucmajor-campo/4445/4446305/es/");
        InmovillaParser.ParseTimestamp(first.ChangedAt).ShouldBe(new DateTimeOffset(2026, 11, 9, 14, 23, 28, TimeSpan.Zero));
        InmovillaParser.ParseTimestamp(first.CreatedAt).ShouldBe(new DateTimeOffset(2016, 10, 28, 0, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Large_page_size_returns_the_whole_catalogue()
    {
        var (page, listings) = InmovillaParser.ParseListPage(Fixtures.Read("inmovilla-native", "mir-venta-res48.html"));

        page.Elements.ShouldBe(36);
        listings.Count.ShouldBe(36);
    }

    [Fact]
    public void Detail_page_adds_description_year_and_condition()
    {
        var html = Fixtures.Read("inmovilla-native", "mir-detail.html");

        var record = InmovillaParser.ParseDetailRecord(html);
        var description = InmovillaParser.ParseDescription(html);

        record.ShouldNotBeNull();
        record.Code.ShouldBe(29483722);
        record.YearBuilt.ShouldBe(1968);
        record.ConditionLabel.ShouldBe("Entrar a vivir");
        record.EnergyLetter.ShouldBe("E");
        record.Elevator.ShouldBe(1);
        description.ShouldStartWith("Apartamento de aproximadamente 36 m²");
    }
}
