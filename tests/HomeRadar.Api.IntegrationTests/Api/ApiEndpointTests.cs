using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HomeRadar.Api.Contracts;
using HomeRadar.Api.IntegrationTests.Support;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.Sources;
using HomeRadar.Domain.ValueObjects;
using HomeRadar.Infrastructure.Persistence;

namespace HomeRadar.Api.IntegrationTests.Api;

[Collection(DatabaseTests.Name)]
public sealed class ApiEndpointTests(DatabaseFixture fixture)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly DateTimeOffset T0 = new(2026, 11, 14, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Requests_without_api_key_are_rejected_with_problem_details()
    {
        await using var factory = new ApiFactory(await fixture.CreateDatabaseAsync("api"));
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/v1/listings", UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
        (await client.GetAsync(new Uri("/health/live", UriKind.Relative), TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Search_filters_sorts_and_paginates_with_cursors()
    {
        var connection = await fixture.CreateDatabaseAsync("api");
        await SeedListingsAsync(connection, 7);
        await using var factory = new ApiFactory(connection);
        using var client = factory.CreateAuthenticatedClient();

        var first = await client.PostAsJsonAsync(new Uri("/v1/listings/search", UriKind.Relative), new { price = new { max = 260000 }, sort = "PriceAsc", limit = 3 }, TestContext.Current.CancellationToken);
        first.StatusCode.ShouldBe(HttpStatusCode.OK);
        var page1 = (await first.Content.ReadFromJsonAsync<PageDto<ListingSummaryDto>>(Json, TestContext.Current.CancellationToken))!;
        page1.Items.Count.ShouldBe(3);
        page1.HasMore.ShouldBeTrue();
        page1.Items.Select(item => item.Price!.Amount).ShouldBeInOrder();

        var second = await client.PostAsJsonAsync(new Uri("/v1/listings/search", UriKind.Relative), new { price = new { max = 260000 }, sort = "PriceAsc", limit = 3, cursor = page1.NextCursor }, TestContext.Current.CancellationToken);
        var page2 = (await second.Content.ReadFromJsonAsync<PageDto<ListingSummaryDto>>(Json, TestContext.Current.CancellationToken))!;
        page2.Items.Select(item => item.Id).ShouldNotContain(page1.Items[0].Id);
        page1.Items.Concat(page2.Items).Select(item => item.Price!.Amount).ShouldBeInOrder();

        var simple = await client.GetFromJsonAsync<PageDto<ListingSummaryDto>>(new Uri("/v1/listings?municipality=inca&limit=50", UriKind.Relative), Json, TestContext.Current.CancellationToken);
        simple!.Items.ShouldAllBe(item => item.Municipality == "Inca");

        var invalid = await client.PostAsJsonAsync(new Uri("/v1/listings/search", UriKind.Relative), new { propertyTypes = new[] { "Castle" }, limit = 500 }, TestContext.Current.CancellationToken);
        invalid.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await invalid.Content.ReadFromJsonAsync<JsonElement>(Json, TestContext.Current.CancellationToken);
        problem.GetProperty("errors").TryGetProperty("limit", out _).ShouldBeTrue();
        problem.GetProperty("errors").TryGetProperty("propertyTypes", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task Listing_detail_and_history_are_served()
    {
        var connection = await fixture.CreateDatabaseAsync("api");
        var ids = await SeedListingsAsync(connection, 1);
        await using var factory = new ApiFactory(connection);
        using var client = factory.CreateAuthenticatedClient();

        var detail = await client.GetFromJsonAsync<ListingDetailDto>(new Uri($"/v1/listings/{ids[0]}", UriKind.Relative), Json, TestContext.Current.CancellationToken);
        detail!.Summary.ExternalId.ShouldBe("ext-0");
        detail.Features.Keys.ShouldNotContain("_fingerprint");
        detail.Photos.Count.ShouldBe(1);

        var history = await client.GetFromJsonAsync<PageDto<ListingEventDto>>(new Uri($"/v1/listings/{ids[0]}/history", UriKind.Relative), Json, TestContext.Current.CancellationToken);
        history!.Items.ShouldHaveSingleItem().Type.ShouldBe("Created");

        (await client.GetAsync(new Uri($"/v1/listings/{Guid.NewGuid()}", UriKind.Relative), TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Financing_profiles_support_idempotent_creation_and_etag_preconditions()
    {
        var connection = await fixture.CreateDatabaseAsync("api");
        await using var factory = new ApiFactory(connection);
        using var client = factory.CreateAuthenticatedClient();
        var input = new
        {
            key = "main",
            name = "Principal",
            savings = new { amount = 60000, currency = "EUR" },
            netMonthlyIncome = new { amount = 3000, currency = "EUR" },
            mortgage = new { type = "Fixed", maxLoanToValue = 0.8, nominalRatePercent = 2.9, termMonths = 360 },
            buyer = new { age = 34 },
            isDefault = true,
        };

        using var create = new HttpRequestMessage(HttpMethod.Post, new Uri("/v1/financing-profiles", UriKind.Relative)) { Content = JsonContent.Create(input) };
        create.Headers.Add("Idempotency-Key", "abc-123");
        using var created = await client.SendAsync(create, TestContext.Current.CancellationToken);
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        var etag = created.Headers.ETag!.Tag;
        var profile = (await created.Content.ReadFromJsonAsync<FinancingProfileDto>(Json, TestContext.Current.CancellationToken))!;

        using var replay = new HttpRequestMessage(HttpMethod.Post, new Uri("/v1/financing-profiles", UriKind.Relative)) { Content = JsonContent.Create(input) };
        replay.Headers.Add("Idempotency-Key", "abc-123");
        using var replayed = await client.SendAsync(replay, TestContext.Current.CancellationToken);
        replayed.StatusCode.ShouldBe(HttpStatusCode.Created, await replayed.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        replayed.Headers.Contains("Idempotent-Replayed").ShouldBeTrue();
        (await replayed.Content.ReadFromJsonAsync<FinancingProfileDto>(Json, TestContext.Current.CancellationToken))!.Id.ShouldBe(profile.Id);

        using var conflict = new HttpRequestMessage(HttpMethod.Post, new Uri("/v1/financing-profiles", UriKind.Relative)) { Content = JsonContent.Create(input with { key = "other" }) };
        conflict.Headers.Add("Idempotency-Key", "abc-123");
        (await client.SendAsync(conflict, TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var update = input with { name = "Renombrado" };
        using var withoutMatch = new HttpRequestMessage(HttpMethod.Put, new Uri($"/v1/financing-profiles/{profile.Id}", UriKind.Relative)) { Content = JsonContent.Create(update) };
        (await client.SendAsync(withoutMatch, TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.PreconditionRequired);

        using var staleMatch = new HttpRequestMessage(HttpMethod.Put, new Uri($"/v1/financing-profiles/{profile.Id}", UriKind.Relative)) { Content = JsonContent.Create(update) };
        staleMatch.Headers.TryAddWithoutValidation("If-Match", "\"999999\"");
        (await client.SendAsync(staleMatch, TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.PreconditionFailed);

        using var goodMatch = new HttpRequestMessage(HttpMethod.Put, new Uri($"/v1/financing-profiles/{profile.Id}", UriKind.Relative)) { Content = JsonContent.Create(update) };
        goodMatch.Headers.TryAddWithoutValidation("If-Match", etag);
        using var updated = await client.SendAsync(goodMatch, TestContext.Current.CancellationToken);
        updated.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await updated.Content.ReadFromJsonAsync<FinancingProfileDto>(Json, TestContext.Current.CancellationToken))!.Name.ShouldBe("Renombrado");
        updated.Headers.ETag!.Tag.ShouldNotBe(etag);
    }

    [Fact]
    public async Task Watchlist_sources_and_readiness_work()
    {
        var connection = await fixture.CreateDatabaseAsync("api");
        var ids = await SeedListingsAsync(connection, 2);
        await using var factory = new ApiFactory(connection);
        using var client = factory.CreateAuthenticatedClient();

        using var added = await client.PostAsJsonAsync(new Uri("/v1/watchlist", UriKind.Relative), new { listingId = ids[0], notes = "visitar" }, TestContext.Current.CancellationToken);
        added.StatusCode.ShouldBe(HttpStatusCode.Created);
        var item = (await added.Content.ReadFromJsonAsync<WatchlistItemDto>(Json, TestContext.Current.CancellationToken))!;
        (await client.PostAsJsonAsync(new Uri("/v1/watchlist", UriKind.Relative), new { listingId = ids[0] }, TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.Conflict);

        using var patch = new HttpRequestMessage(HttpMethod.Patch, new Uri($"/v1/watchlist/{item.Id}", UriKind.Relative)) { Content = JsonContent.Create(new { status = "Visited" }) };
        patch.Headers.TryAddWithoutValidation("If-Match", added.Headers.ETag!.Tag);
        using var patched = await client.SendAsync(patch, TestContext.Current.CancellationToken);
        patched.StatusCode.ShouldBe(HttpStatusCode.OK, $"etag={added.Headers.ETag!.Tag} body={await patched.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)}");
        (await patched.Content.ReadFromJsonAsync<WatchlistItemDto>(Json, TestContext.Current.CancellationToken))!.Status.ShouldBe("Visited");

        var sources = await client.GetFromJsonAsync<List<SourceDto>>(new Uri("/v1/sources", UriKind.Relative), Json, TestContext.Current.CancellationToken);
        sources!.ShouldHaveSingleItem().Key.ShouldBe("test");
        var health = await client.GetFromJsonAsync<SourceHealthDto>(new Uri($"/v1/sources/{sources[0].Id}/health", UriKind.Relative), Json, TestContext.Current.CancellationToken);
        health!.Source.Status.ShouldBe("Healthy");

        (await client.GetAsync(new Uri("/health/ready", UriKind.Relative), TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await client.GetAsync(new Uri("/openapi/v1.json", UriKind.Relative), TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private static async Task<List<Guid>> SeedListingsAsync(string connection, int count)
    {
        await using var db = new HomeRadarDbContext(DatabaseFixture.BuildOptions(connection));
        var source = new Source("test", "Test source", SourceKind.Agency, SellerType.Agency, "fake", TimeSpan.FromHours(1), false, "{}");
        db.Sources.Add(source);
        var ids = new List<Guid>();
        for (var index = 0; index < count; index++)
        {
            var listing = Listing.Create(source.Id, source.Key, source.SellerType, new ListingObservation
            {
                ExternalId = $"ext-{index}",
                Url = new Uri($"https://example.test/{index}"),
                Title = $"Piso {index}",
                Price = Money.Euros(200_000 + index * 15_000),
                BuiltArea = new Area(80 + index * 5),
                Rooms = 2 + index % 3,
                PropertyType = PropertyType.Flat,
                Municipality = index % 2 == 0 ? "Inca" : "Palma",
                Features = new Dictionary<string, string>(StringComparer.Ordinal) { ["_fingerprint"] = "x", ["elevator"] = "true" },
                PhotoUrls = [new Uri($"https://example.test/{index}.jpg")],
            }, T0.AddHours(index));
            db.Listings.Add(listing);
            ids.Add(listing.Id);
        }

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return ids;
    }
}
