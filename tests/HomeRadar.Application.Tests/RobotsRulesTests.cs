using HomeRadar.Infrastructure.Http;

namespace HomeRadar.Application.Tests;

public sealed class RobotsRulesTests
{
    private const string Inmovilla = """
        User-agent: *
        Crawl-delay: 6

        Allow: /
        Disallow: /js/
        Disallow: /lib/

        User-agent: AhrefsBot
        Disallow: /
        """;

    private const string Solvia = """
        User-agent: * 
        Disallow: /api/ 
        Disallow: /ajax/ 
        Sitemap: https://www.solvia.es/sitemap.xml
        """;

    [Fact]
    public void Longest_match_wins_so_allow_root_does_not_override_disallowed_folders()
    {
        var rules = RobotsRules.Parse(Inmovilla, "HomeRadarBot");

        rules.IsAllowed("/venta/es/?res=48&pag=2").ShouldBeTrue();
        rules.IsAllowed("/js/modulos/paginacion/paginacion.js").ShouldBeFalse();
        rules.CrawlDelay.ShouldBe(TimeSpan.FromSeconds(6));
    }

    [Fact]
    public void Api_paths_are_refused_for_solvia_while_listing_pages_are_allowed()
    {
        var rules = RobotsRules.Parse(Solvia, "HomeRadarBot");

        rules.IsAllowed("/es/comprar/viviendas/balears-illes?filtro.minPrecio=:ge:0").ShouldBeTrue();
        rules.IsAllowed("/api/inmuebles/v2/buscarInmuebles").ShouldBeFalse();
        rules.CrawlDelay.ShouldBeNull();
    }

    [Fact]
    public void A_group_naming_our_bot_takes_precedence_over_the_wildcard()
    {
        var rules = RobotsRules.Parse("User-agent: *\nDisallow: /\n\nUser-agent: HomeRadarBot\nAllow: /public/\nDisallow: /", "HomeRadarBot/0.1");

        rules.IsAllowed("/public/list").ShouldBeTrue();
        rules.IsAllowed("/private").ShouldBeFalse();
    }

    [Fact]
    public void Wildcards_and_anchors_are_supported()
    {
        var rules = RobotsRules.Parse("User-agent: *\nDisallow: /*.pdf$\nDisallow: /search*sort=", "HomeRadarBot");

        rules.IsAllowed("/docs/file.pdf").ShouldBeFalse();
        rules.IsAllowed("/docs/file.pdfx").ShouldBeTrue();
        rules.IsAllowed("/search?q=a&sort=price").ShouldBeFalse();
        rules.IsAllowed("/search?q=a").ShouldBeTrue();
    }
}
