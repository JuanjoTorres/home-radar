using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HomeRadar.Api.Contracts;
using HomeRadar.Api.IntegrationTests.Support;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.Sources;
using HomeRadar.Domain.ValueObjects;
using HomeRadar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HomeRadar.Api.IntegrationTests.Api;

[Collection(DatabaseTests.Name)]
public sealed class IntelligenceEndpointTests(DatabaseFixture fixture)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly DateTimeOffset T0 = new(2026, 11, 14, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Analysis_includes_renovation_red_flags_negotiation_score_and_bid()
    {
        var connection = await fixture.CreateDatabaseAsync("intelligence");
        var listings = await SeedAsync(connection);
        await using var factory = new ApiFactory(connection);
        using var client = factory.CreateAuthenticatedClient();

        var toRenovate = await client.GetFromJsonAsync<ListingAnalysisDto>(new Uri($"/v1/listings/{listings["renovate"]}/analysis", UriKind.Relative), Json, TestContext.Current.CancellationToken);
        var occupied = await client.GetFromJsonAsync<ListingAnalysisDto>(new Uri($"/v1/listings/{listings["occupied"]}/analysis", UriKind.Relative), Json, TestContext.Current.CancellationToken);

        var intelligence = toRenovate!.Intelligence.ShouldNotBeNull();
        intelligence.Renovation.Level.ShouldBe("Full");
        intelligence.Renovation.Probable.Amount.ShouldBe(102_069m);
        toRenovate.Result.Base.Costs.ShouldContain(cost => cost.Category == "Renovation" && cost.Amount.Amount == 102_069m);
        intelligence.Negotiation.Applicable.ShouldBeTrue();
        intelligence.Negotiation.Factors.ShouldContain(factor => factor.Code == "needsRenovation");
        intelligence.Score.Value.ShouldBeInRange(1, 100);
        intelligence.Bid.WalkAwayPrice.ShouldNotBeNull();
        occupied!.Intelligence!.RedFlags.ShouldContain(flag => flag.Code == "occupied" && flag.Severity == "Critical");
        occupied.Intelligence.Negotiation.Applicable.ShouldBeFalse();
        occupied.Intelligence.Score.Value.ShouldBeLessThanOrEqualTo(30);
        factory.Cadastre.Calls.ShouldBe(0);
    }

    [Fact]
    public async Task Manual_renovation_prevails_until_it_is_deleted()
    {
        var connection = await fixture.CreateDatabaseAsync("intelligence");
        var listings = await SeedAsync(connection);
        await using var factory = new ApiFactory(connection);
        using var client = factory.CreateAuthenticatedClient();
        var route = new Uri($"/v1/listings/{listings["renovate"]}/renovation-override", UriKind.Relative);

        using var set = await client.PutAsJsonAsync(route, new { items = new[] { new { code = "kitchen", description = "Cocina", amount = new { amount = 9_000 } }, new { code = "bathroom", description = "Baño", amount = new { amount = 5_500 } } }, notes = "Visita 14/11" }, TestContext.Current.CancellationToken);
        set.StatusCode.ShouldBe(HttpStatusCode.OK);
        var manual = (await set.Content.ReadFromJsonAsync<RenovationEstimateDto>(Json, TestContext.Current.CancellationToken))!;
        manual.Source.ShouldBe("Manual");
        manual.Probable.Amount.ShouldBe(14_500m);
        var etag = set.Headers.ETag.ShouldNotBeNull();

        using var stale = new HttpRequestMessage(HttpMethod.Put, route) { Content = JsonContent.Create(new { level = "Light" }) };
        stale.Headers.TryAddWithoutValidation("If-Match", "\"1\"");
        (await client.SendAsync(stale, TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.PreconditionFailed);
        using var fresh = new HttpRequestMessage(HttpMethod.Put, route) { Content = JsonContent.Create(new { level = "Light" }) };
        fresh.Headers.IfMatch.Add(etag);
        (await client.SendAsync(fresh, TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.OK);

        var analysis = await client.GetFromJsonAsync<ListingAnalysisDto>(new Uri($"/v1/listings/{listings["renovate"]}/analysis", UriKind.Relative), Json, TestContext.Current.CancellationToken);
        analysis!.Intelligence!.Renovation.Level.ShouldBe("Light");
        analysis.Intelligence.Renovation.Source.ShouldBe("Manual");

        (await client.DeleteAsync(route, TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await client.DeleteAsync(route, TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var invalid = await client.PutAsJsonAsync(route, new { level = "Palace" }, TestContext.Current.CancellationToken);
        invalid.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Cadastral_data_and_reference_value_feed_flags_and_taxes()
    {
        var connection = await fixture.CreateDatabaseAsync("intelligence");
        var listings = await SeedAsync(connection);
        await using var factory = new ApiFactory(connection);
        using var client = factory.CreateAuthenticatedClient();

        using var response = await client.PutAsJsonAsync(new Uri($"/v1/listings/{listings["ready"]}/cadastral-data", UriKind.Relative), new
        {
            cadastralReference = "0763604DD7806C0002IU",
            builtSquareMetres = 65,
            yearBuilt = 2012,
            referenceValue = new { amount = 260_000 },
            referenceValueDate = "2026-11-14",
        }, TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var data = (await response.Content.ReadFromJsonAsync<CadastralDataDto>(Json, TestContext.Current.CancellationToken))!;
        data.Cadastre!.Match.ShouldBe("Manual");

        var analysis = await client.GetFromJsonAsync<ListingAnalysisDto>(new Uri($"/v1/listings/{listings["ready"]}/analysis", UriKind.Relative), Json, TestContext.Current.CancellationToken);
        analysis!.Intelligence!.RedFlags.ShouldContain(flag => flag.Code == "areaMismatchHigh");
        analysis.Result.Base.Taxes.Lines[0].TaxBase.Amount.ShouldBe(260_000m);

        var invalid = await client.PutAsJsonAsync(new Uri($"/v1/listings/{listings["ready"]}/cadastral-data", UriKind.Relative), new { cadastralReference = "nope" }, TestContext.Current.CancellationToken);
        invalid.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Renovation_can_be_estimated_without_a_listing()
    {
        var connection = await fixture.CreateDatabaseAsync("intelligence");
        await SeedAsync(connection);
        await using var factory = new ApiFactory(connection);
        using var client = factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync(new Uri("/v1/renovation/estimates", UriKind.Relative), new { builtSquareMetres = 80, bathrooms = 1, rooms = 2, yearBuilt = 1975, description = "Piso a reformar" }, TestContext.Current.CancellationToken);
        var empty = await client.PostAsJsonAsync(new Uri("/v1/renovation/estimates", UriKind.Relative), new { }, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var estimate = (await response.Content.ReadFromJsonAsync<RenovationEstimateDto>(Json, TestContext.Current.CancellationToken))!;
        estimate.Level.ShouldBe("Full");
        estimate.Probable.Amount.ShouldBe(90_728m);
        estimate.Lines.ShouldContain(line => line.Code == "kitchen");
        empty.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_filters_by_score_renovation_and_red_flags_and_sorts_by_score()
    {
        var connection = await fixture.CreateDatabaseAsync("intelligence");
        var listings = await SeedAsync(connection);
        await using var factory = new ApiFactory(connection);
        using var client = factory.CreateAuthenticatedClient();
        foreach (var id in listings.Values)
        {
            await client.GetAsync(new Uri($"/v1/listings/{id}/analysis", UriKind.Relative), TestContext.Current.CancellationToken);
        }

        var byLevel = await SearchAsync(client, new { renovationLevels = new[] { "Full" } });
        var withoutCritical = await SearchAsync(client, new { excludeRedFlags = new[] { "occupied" } });
        var sorted = await SearchAsync(client, new { sort = "ScoreDesc" });
        var minimum = await SearchAsync(client, new { minScore = 31 });
        var invalid = await client.PostAsJsonAsync(new Uri("/v1/listings/search", UriKind.Relative), new { renovationLevels = new[] { "Palace" }, minScore = 101 }, TestContext.Current.CancellationToken);

        byLevel.Items.Select(item => item.Id).ShouldBe([listings["renovate"]]);
        byLevel.Items[0].RenovationLevel.ShouldBe("Full");
        withoutCritical.Items.Select(item => item.Id).ShouldNotContain(listings["occupied"]);
        sorted.Items.Select(item => item.Score ?? -1).ShouldBeInOrder(SortDirection.Descending);
        sorted.Items.Count.ShouldBe(3);
        minimum.Items.Select(item => item.Id).ShouldNotContain(listings["occupied"]);
        invalid.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private static async Task<PageDto<ListingSummaryDto>> SearchAsync(HttpClient client, object request)
    {
        var response = await client.PostAsJsonAsync(new Uri("/v1/listings/search", UriKind.Relative), request, TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<PageDto<ListingSummaryDto>>(Json, TestContext.Current.CancellationToken))!;
    }

    private static async Task<Dictionary<string, Guid>> SeedAsync(string connection)
    {
        await using var db = new HomeRadarDbContext(DatabaseFixture.BuildOptions(connection));
        await FinanceSeed.ApplyAsync(db, savings: 200_000m);
        var source = new Source("test", "Test source", SourceKind.Agency, SellerType.Agency, "fake", TimeSpan.FromHours(1), false, "{}");
        db.Sources.Add(source);
        var listings = new Dictionary<string, Guid>(StringComparer.Ordinal);
        foreach (var (key, price, year, description) in new[]
        {
            ("ready", 240_000m, "2015", "Piso para entrar a vivir"),
            ("renovate", 180_000m, "1972", "Piso a reformar con cocina original. Precio negociable."),
            ("occupied", 120_000m, "1990", "Vivienda ocupada, solo inversores"),
        })
        {
            var listing = Listing.Create(source.Id, source.Key, source.SellerType, new ListingObservation
            {
                ExternalId = key,
                Url = new Uri($"https://example.test/{key}"),
                Title = $"Piso {key}",
                Description = description,
                Price = Money.Euros(price),
                BuiltArea = new Area(90),
                Rooms = 3,
                Bathrooms = 1,
                PropertyType = PropertyType.Flat,
                Municipality = "Inca",
                Features = new Dictionary<string, string>(StringComparer.Ordinal) { ["_fingerprint"] = key, [FeatureKeys.YearBuilt] = year },
            }, T0);
            db.Listings.Add(listing);
            listings[key] = listing.Id;
        }

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return listings;
    }
}
