using HomeRadar.Sources.Aliseda;
using HomeRadar.Sources.ContractTests.Support;

namespace HomeRadar.Sources.ContractTests;

public sealed class AlisedaParserTests
{
    [Fact]
    public void List_page_embeds_the_search_response_with_pagination()
    {
        var page = AlisedaParser.ParseSearchPage(Fixtures.Read("aliseda", "list-balears.html"));

        page.CurrentPage.ShouldBe(1);
        page.LastPage.ShouldBe(2);
        page.Total.ShouldBe(18);
        page.Data.Count.ShouldBe(12);
        var first = page.Data[0];
        first.Id.ShouldBe("16276991");
        first.CadastralReference.ShouldBe("6074901DD8567S0041EP");
        first.Possession.ShouldBe("LIBRE");
        first.Operation!.Price.ShouldBe(2035m);
        first.Operation.PreviousPrice.ShouldBe(2125m);
        first.Address!.Latitude.ShouldNotBeNull();
        first.Address.City.ShouldBe("Llucmajor");
        first.Images.Count.ShouldBe(5);
        first.Description!.ShouldContain("Cala Pi");
        AlisedaParser.ParseDate(first.Operation.PublishedOn).ShouldBe(new DateTimeOffset(2012, 10, 15, 0, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Second_page_holds_the_remaining_listings()
    {
        var page = AlisedaParser.ParseSearchPage(Fixtures.Read("aliseda", "list-balears-page2.html"));

        page.CurrentPage.ShouldBe(2);
        page.Data.Count.ShouldBe(6);
    }
}
