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
public sealed class AnalysisEndpointTests(DatabaseFixture fixture)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly DateTimeOffset T0 = new(2026, 11, 14, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Listing_analysis_is_computed_stored_and_reused()
    {
        var connection = await fixture.CreateDatabaseAsync("analysis");
        var (listings, profileId) = await SeedAsync(connection);
        await using var factory = new ApiFactory(connection);
        using var client = factory.CreateAuthenticatedClient();

        var first = await client.GetFromJsonAsync<ListingAnalysisDto>(new Uri($"/v1/listings/{listings[250_000]}/analysis", UriKind.Relative), Json, TestContext.Current.CancellationToken);
        var second = await client.GetFromJsonAsync<ListingAnalysisDto>(new Uri($"/v1/listings/{listings[250_000]}/analysis?financingProfileId={profileId}", UriKind.Relative), Json, TestContext.Current.CancellationToken);

        first!.Reused.ShouldBeFalse();
        first.FinancingProfileId.ShouldBe(profileId);
        first.Result.Base.Verdict.ShouldBe("Affordable");
        first.Result.Base.TotalAcquisitionCost.Amount.ShouldBe(256_675.69m);
        first.Result.Base.Taxes.Lines.ShouldHaveSingleItem().RuleId.ShouldBe("itp_under36_first_home_2");
        first.Result.MaximumPrice!.Price.ShouldNotBeNull();
        first.Configuration.TaxRuleSets.Select(set => set.Jurisdiction).ShouldBe(["ES", "ES-IB"], ignoreOrder: true);
        second!.Reused.ShouldBeTrue();
        second.AnalysisId.ShouldBe(first.AnalysisId);

        await using var db = new HomeRadarDbContext(DatabaseFixture.BuildOptions(connection));
        (await db.ListingAnalyses.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }

    [Fact]
    public async Task Analysis_errors_are_problem_details()
    {
        var connection = await fixture.CreateDatabaseAsync("analysis");
        var (listings, _) = await SeedAsync(connection);
        await using var factory = new ApiFactory(connection);
        using var client = factory.CreateAuthenticatedClient();

        using var noPrice = await client.GetAsync(new Uri($"/v1/listings/{listings[0]}/analysis", UriKind.Relative), TestContext.Current.CancellationToken);
        using var unknownProfile = await client.GetAsync(new Uri($"/v1/listings/{listings[250_000]}/analysis?financingProfileId={Guid.NewGuid()}", UriKind.Relative), TestContext.Current.CancellationToken);
        using var unknownListing = await client.GetAsync(new Uri($"/v1/listings/{Guid.NewGuid()}/analysis", UriKind.Relative), TestContext.Current.CancellationToken);

        noPrice.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await noPrice.Content.ReadFromJsonAsync<JsonElement>(Json, TestContext.Current.CancellationToken)).GetProperty("code").GetString().ShouldBe("listing.noPrice");
        unknownProfile.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        unknownListing.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Simulation_and_maximum_price_work_without_a_listing()
    {
        var connection = await fixture.CreateDatabaseAsync("analysis");
        await SeedAsync(connection);
        await using var factory = new ApiFactory(connection);
        using var client = factory.CreateAuthenticatedClient();

        var simulation = await client.PostAsJsonAsync(new Uri("/v1/analysis/simulations", UriKind.Relative), new
        {
            price = new { amount = 300_000 },
            municipality = "Inca",
            renovationCost = new { amount = 15_000 },
            financing = new
            {
                savings = new { amount = 80_000 },
                netMonthlyIncome = new { amount = 3_200 },
                mortgage = new { type = "Variable", maxLoanToValue = 0.8, spreadPercent = 0.6, termMonths = 360 },
                buyer = new { age = 40 },
            },
            euriborPercent = 2.5,
        }, TestContext.Current.CancellationToken);
        simulation.StatusCode.ShouldBe(HttpStatusCode.OK);
        var simulated = (await simulation.Content.ReadFromJsonAsync<SimulationResultDto>(Json, TestContext.Current.CancellationToken))!;
        simulated.EuriborPercent.ShouldBe(2.5m);
        simulated.Result.Base.Taxes.Lines.ShouldHaveSingleItem().Quota.Amount.ShouldBe(13_193.95m);
        simulated.Result.Base.Payments.Select(schedule => schedule.Scenario).ShouldBe(["base", "plus1", "plus2"]);
        simulated.Result.Base.Costs.ShouldContain(cost => cost.Category == "RenovationContingency" && cost.Amount.Amount == 2_250m);
        (simulated.Margins.MaximumAffordableRenovation is null).ShouldBe(simulated.Result.Base.Verdict != "Affordable");
        simulated.Margins.RenovatedMarketValue.ShouldBeNull();

        var maximum = await client.PostAsJsonAsync(new Uri("/v1/affordability/maximum-price", UriKind.Relative), new { municipality = "Palma" }, TestContext.Current.CancellationToken);
        maximum.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = (await maximum.Content.ReadFromJsonAsync<MaximumPriceResultDto>(Json, TestContext.Current.CancellationToken))!;
        result.Maximum.Price!.Amount.ShouldBeGreaterThan(250_000m);
        result.AtMaximum!.Verdict.ShouldBe("Affordable");
        result.Maximum.BindingConstraints.ShouldContain(reason => reason.Code == "CashShortfall");

        var invalid = await client.PostAsJsonAsync(new Uri("/v1/analysis/simulations", UriKind.Relative), new { price = new { amount = -1 }, purchaseKind = "Rental" }, TestContext.Current.CancellationToken);
        invalid.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_returns_only_affordable_listings()
    {
        var connection = await fixture.CreateDatabaseAsync("analysis");
        var (listings, profileId) = await SeedAsync(connection);
        await using var factory = new ApiFactory(connection);
        using var client = factory.CreateAuthenticatedClient();

        var byProfile = await client.PostAsJsonAsync(new Uri("/v1/listings/search", UriKind.Relative), new { affordability = new { financingProfileId = profileId }, limit = 50 }, TestContext.Current.CancellationToken);
        var byBudget = await client.GetFromJsonAsync<PageDto<ListingSummaryDto>>(new Uri("/v1/listings?budget=330000&limit=50", UriKind.Relative), Json, TestContext.Current.CancellationToken);

        byProfile.StatusCode.ShouldBe(HttpStatusCode.OK);
        var affordable = (await byProfile.Content.ReadFromJsonAsync<PageDto<ListingSummaryDto>>(Json, TestContext.Current.CancellationToken))!;
        affordable.Items.Select(item => item.Id).ShouldBe([listings[250_000]]);
        byBudget!.Items.Select(item => item.Id).ShouldBe([listings[300_000], listings[250_000]], ignoreOrder: true);
    }

    [Fact]
    public async Task Configuration_is_exposed_for_auditing()
    {
        var connection = await fixture.CreateDatabaseAsync("analysis");
        await SeedAsync(connection);
        await using var factory = new ApiFactory(connection);
        using var client = factory.CreateAuthenticatedClient();

        var config = await client.GetFromJsonAsync<BusinessConfigDto>(new Uri("/v1/business-config", UriKind.Relative), Json, TestContext.Current.CancellationToken);
        var versions = await client.GetFromJsonAsync<List<BusinessConfigDto>>(new Uri("/v1/business-config/versions", UriKind.Relative), Json, TestContext.Current.CancellationToken);
        var rules = await client.GetFromJsonAsync<List<TaxRuleSetDto>>(new Uri("/v1/tax-rules?jurisdiction=ES-IB&on=2026-11-15", UriKind.Relative), Json, TestContext.Current.CancellationToken);

        config!.Euribor.TwelveMonthPercent.ShouldBe(3.239m);
        versions!.Count.ShouldBe(2);
        versions[0].Id.ShouldBe(config.Id);
        config.Affordability.RenovationContingencyPercent.ShouldBe(15m);
        rules!.Select(set => set.Jurisdiction).ShouldBe(["ES", "ES-IB"]);
        rules![1].Document.GetProperty("rules").GetArrayLength().ShouldBeGreaterThan(10);
    }

    private static async Task<(Dictionary<int, Guid> Listings, Guid ProfileId)> SeedAsync(string connection)
    {
        await using var db = new HomeRadarDbContext(DatabaseFixture.BuildOptions(connection));
        var profile = await FinanceSeed.ApplyAsync(db);
        var source = new Source("test", "Test source", SourceKind.Agency, SellerType.Agency, "fake", TimeSpan.FromHours(1), false, "{}");
        db.Sources.Add(source);
        var listings = new Dictionary<int, Guid>();
        foreach (var price in new[] { 0, 250_000, 300_000, 450_000 })
        {
            var listing = Listing.Create(source.Id, source.Key, source.SellerType, new ListingObservation
            {
                ExternalId = $"ext-{price}",
                Url = new Uri($"https://example.test/{price}"),
                Title = $"Piso {price}",
                Price = price == 0 ? null : Money.Euros(price),
                BuiltArea = new Area(90),
                Rooms = 3,
                PropertyType = PropertyType.Flat,
                Municipality = "Palma",
                Features = new Dictionary<string, string>(StringComparer.Ordinal) { ["_fingerprint"] = $"x{price}", [FeatureKeys.YearBuilt] = "2015" },
            }, T0);
            db.Listings.Add(listing);
            listings[price] = listing.Id;
        }

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return (listings, profile.Id);
    }
}
