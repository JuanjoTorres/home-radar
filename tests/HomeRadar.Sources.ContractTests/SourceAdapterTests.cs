using HomeRadar.Application.Ports;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.Sources;
using HomeRadar.Domain.ValueObjects;
using HomeRadar.Sources.Aliseda;
using HomeRadar.Sources.ContractTests.Support;
using HomeRadar.Sources.Diglo;
using HomeRadar.Sources.Inmovilla;
using HomeRadar.Sources.Solvia;
using Microsoft.Extensions.Logging.Abstractions;

namespace HomeRadar.Sources.ContractTests;

public sealed class SourceAdapterTests
{
    private static SourceFetchContext Context(string key, string adapter, string configuration, bool fullScan, Func<string, string?>? fingerprints = null) =>
        new(
            new Source(key, key, SourceKind.Servicer, SellerType.Servicer, adapter, TimeSpan.FromHours(6), true, configuration),
            fullScan,
            null,
            (externalId, _) => Task.FromResult(fingerprints?.Invoke(externalId)));

    [Fact]
    public async Task Solvia_quick_scan_reads_the_first_page_and_details_for_new_listings()
    {
        var http = new FakeSourceHttpClient()
            .Map(url => url.AbsolutePath.EndsWith("/balears-illes", StringComparison.Ordinal) && string.IsNullOrEmpty(url.Query), "solvia", "list-balears.html")
            .Map(url => url.AbsolutePath.EndsWith("-38415-48244", StringComparison.Ordinal), "solvia", "detail.html");
        var source = new SolviaSource(http, NullLogger<SolviaSource>.Instance);

        var results = await source.FetchAsync(Context("solvia", "solvia", "{}", fullScan: false), TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

        results.Count.ShouldBe(1);
        var observation = results[0].Observation!;
        observation.ExternalId.ShouldBe("38415-48244");
        observation.Price!.Value.Amount.ShouldBe(1_495_000m);
        observation.CadastralReference!.Value.Value.ShouldBe("9044402CD4194N0010LZ");
        observation.Description.ShouldNotBeNullOrWhiteSpace();
        observation.PhotoUrls.Count.ShouldBeGreaterThan(5);
        observation.Features[FeatureKeys.NewBuild].ShouldBe("true");
        observation.Features[FeatureKeys.PreviousPrice].ShouldBe("1590000");
        observation.PublishedAt!.Value.Year.ShouldBe(2025);
        observation.ComputeContentHash().Value.Length.ShouldBe(64);
    }

    [Fact]
    public async Task Solvia_unchanged_fingerprint_skips_the_detail_page()
    {
        var http = new FakeSourceHttpClient()
            .Map(url => url.AbsolutePath.EndsWith("-38415-48244", StringComparison.Ordinal), "solvia", "detail.html")
            .Map(url => url.AbsolutePath.EndsWith("/balears-illes", StringComparison.Ordinal), "solvia", "list-balears.html");
        var source = new SolviaSource(http, NullLogger<SolviaSource>.Instance);
        var firstPass = await source.FetchAsync(Context("solvia", "solvia", "{}", fullScan: false), TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);
        var fingerprint = firstPass[0].Observation!.Features[FeatureKeys.Fingerprint];

        var secondPass = await source.FetchAsync(Context("solvia", "solvia", "{}", fullScan: false, id => id == "38415-48244" ? fingerprint : null), TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

        secondPass.ShouldContain(result => result.ExternalId == "38415-48244" && result.IsUnchanged);
        http.Requests.Count(url => url.AbsolutePath.EndsWith("-38415-48244", StringComparison.Ordinal)).ShouldBe(1);
    }

    [Fact]
    public async Task Solvia_full_scan_bisects_price_bands_until_pages_are_not_full()
    {
        var http = new FakeSourceHttpClient().Map(url => url.Query.Contains("minPrecio", StringComparison.Ordinal), "solvia", "list-balears.html");
        var source = new SolviaSource(http, NullLogger<SolviaSource>.Instance);
        var configuration = "{\"fetchDetails\":false,\"maxPrice\":80000,\"minBandWidth\":20000}";

        var results = await source.FetchAsync(Context("solvia", "solvia", configuration, fullScan: true), TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

        http.Requests.Count.ShouldBeGreaterThan(3);
        http.Requests.ShouldAllBe(url => url.Query.Contains("filtro.minPrecio=:ge:", StringComparison.Ordinal));
        results.Select(r => r.ExternalId).Distinct().Count().ShouldBe(20);
    }

    [Fact]
    public async Task Aliseda_walks_every_page_of_the_embedded_search()
    {
        var http = new FakeSourceHttpClient()
            .Map(url => url.Query.Contains("page=1", StringComparison.Ordinal), "aliseda", "list-balears.html")
            .Map(url => url.Query.Contains("page=2", StringComparison.Ordinal), "aliseda", "list-balears-page2.html");
        var source = new AlisedaSource(http);

        var results = await source.FetchAsync(Context("aliseda", "aliseda", "{}", fullScan: true), TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

        results.Count.ShouldBe(18);
        http.Requests.Count.ShouldBe(2);
        var first = results[0].Observation!;
        first.ExternalId.ShouldBe("16276991");
        first.Possession.ShouldBe(PossessionStatus.Vacant);
        first.Location!.Y.ShouldBe(39.3649609541995, 1e-9);
        first.CadastralReference!.Value.Value.ShouldBe("6074901DD8567S0041EP");
        first.Features[FeatureKeys.PostalCode].ShouldBe("07620");
        first.PhotoUrls.Count.ShouldBe(4);
        first.Url.ToString().ShouldBe("https://www.alisedainmobiliaria.com/inmueble/16276991");
        results.ShouldAllBe(r => r.Observation!.Price != null);
    }

    [Fact]
    public async Task Diglo_fetches_details_for_new_listings_and_stops_at_the_last_page()
    {
        var http = new FakeSourceHttpClient()
            .Map(url => string.IsNullOrEmpty(url.Query) && url.AbsolutePath.EndsWith("/islas-baleares", StringComparison.Ordinal), "diglo", "list-balears.html")
            .Map(url => url.Query.Contains("page=1", StringComparison.Ordinal), "diglo", "list-balears-page1.html")
            .Map(url => url.AbsolutePath.EndsWith("/ret0000206481", StringComparison.OrdinalIgnoreCase), "diglo", "detail.html");
        var source = new DigloSource(http, NullLogger<DigloSource>.Instance);

        var results = await source.FetchAsync(Context("diglo", "diglo", "{}", fullScan: true), TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

        results.Count.ShouldBe(1);
        var observation = results[0].Observation!;
        observation.ExternalId.ShouldBe("RET0000206481");
        observation.Location!.Y.ShouldBe(39.540443668877, 1e-9);
        observation.Description.ShouldStartWith("Local Comercial");
        observation.PropertyType.ShouldBe(PropertyType.Commercial);
        observation.Features["cadastralParcel"].ShouldBe("8770001ED2787S");
        http.Requests.Count(url => url.Query.Contains("page=", StringComparison.Ordinal)).ShouldBe(2);
    }

    [Fact]
    public async Task Inmovilla_reads_the_embedded_catalogue_and_enriches_from_detail()
    {
        var http = new FakeSourceHttpClient()
            .Map(url => url.Query.Contains("pag=1", StringComparison.Ordinal), "inmovilla-native", "mir-venta-res48.html")
            .Map(url => url.AbsolutePath.Contains("/4445/29483722/", StringComparison.Ordinal), "inmovilla-native", "mir-detail.html");
        var source = new InmovillaNativeSource(http, NullLogger<InmovillaNativeSource>.Instance);
        var configuration = "{\"baseUrl\":\"https://www.inmobiliariamir.es/\"}";

        var results = await source.FetchAsync(Context("inmovilla-mir", "inmovilla", configuration, fullScan: true), TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

        results.Count.ShouldBe(1);
        var observation = results[0].Observation!;
        observation.ExternalId.ShouldBe("29483722");
        observation.Price!.Value.Amount.ShouldBe(215_000m);
        observation.Features[FeatureKeys.PreviousPrice].ShouldBe("225000");
        observation.Features[FeatureKeys.YearBuilt].ShouldBe("1968");
        observation.Features[FeatureKeys.Condition].ShouldBe("Entrar a vivir");
        observation.Description.ShouldStartWith("Apartamento");
        observation.PhotoUrls.Count.ShouldBe(15);
        observation.PhotoUrls[0].ToString().ShouldBe("https://fotos15.apinmo.com/4445/29483722/4-1.jpg");
        observation.Municipality.ShouldBe("Llucmajor");
        observation.Location.ShouldNotBeNull();
        observation.PublishedAt.ShouldNotBeNull();
        http.Requests.Count(url => url.Query.Contains("pag=", StringComparison.Ordinal)).ShouldBe(1);
    }
}
