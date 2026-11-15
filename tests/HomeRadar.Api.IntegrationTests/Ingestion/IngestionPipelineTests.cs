using HomeRadar.Api.IntegrationTests.Support;
using HomeRadar.Application.Common;
using HomeRadar.Application.Ingestion;
using HomeRadar.Application.Notifications;
using HomeRadar.Application.Outbox;
using HomeRadar.Application.Ports;
using HomeRadar.Domain.Finance;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.Sources;
using HomeRadar.Domain.ValueObjects;
using HomeRadar.Domain.Watchlist;
using HomeRadar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace HomeRadar.Api.IntegrationTests.Ingestion;

[Collection(DatabaseTests.Name)]
public sealed class IngestionPipelineTests(DatabaseFixture fixture)
{
    private static readonly DateTimeOffset T0 = new(2026, 11, 14, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task New_listing_is_stored_with_snapshot_event_and_matching_alert_then_changes_are_tracked()
    {
        var connection = await fixture.CreateDatabaseAsync("ingest");
        var adapter = new FakeAdapter();
        var clock = new FakeTimeProvider(T0);
        var sent = new List<Notification>();
        await using var provider = BuildServices(connection, adapter, clock, sent);

        Guid sourceId;
        await using (var scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HomeRadarDbContext>();
            var source = new Source("fake", "Fake portal", SourceKind.Servicer, SellerType.Servicer, "fake", TimeSpan.FromHours(1), true, "{}");
            db.Sources.Add(source);
            db.SearchProfiles.Add(new SearchProfile("inca", 1, "Inca hasta 260k", new ListingFilter { PriceMax = 260_000, Municipalities = ["Inca"] }, [AlertChannel.Telegram], true, true));
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            sourceId = source.Id;
        }

        adapter.Listings = [Observation("1", 245_000, "Piso en Inca"), Observation("2", 420_000, "Casa en Pollença", "Pollença")];
        await RunAsync(provider, sourceId);
        await DispatchAsync(provider);

        await using (var scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HomeRadarDbContext>();
            (await db.Listings.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(2);
            (await db.ListingSnapshots.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(2);
            (await db.ListingEvents.CountAsync(e => e.Type == ListingEventType.Created, TestContext.Current.CancellationToken)).ShouldBe(2);
            var run = await db.SourceRuns.SingleAsync(TestContext.Current.CancellationToken);
            run.Outcome.ShouldBe(SourceRunOutcome.Succeeded);
            run.ListingsCreated.ShouldBe(2);
            var inca = await db.Listings.SingleAsync(l => l.ExternalId == "1", TestContext.Current.CancellationToken);
            db.WatchlistItems.Add(new WatchlistItem(inca.Id, "visitar", T0));
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        sent.ShouldHaveSingleItem().Body.ShouldContain("Piso en Inca");
        sent.Clear();

        clock.Advance(TimeSpan.FromDays(3));
        adapter.Listings = [Observation("1", 229_000, "Piso en Inca")];
        await RunAsync(provider, sourceId);
        await DispatchAsync(provider);

        await using (var scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HomeRadarDbContext>();
            var inca = await db.Listings.SingleAsync(l => l.ExternalId == "1", TestContext.Current.CancellationToken);
            inca.PriceAmount.ShouldBe(229_000m);
            inca.PriceDropCount.ShouldBe(1);
            var pollenca = await db.Listings.SingleAsync(l => l.ExternalId == "2", TestContext.Current.CancellationToken);
            pollenca.Status.ShouldBe(ListingStatus.Withdrawn);
            (await db.ListingSnapshots.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(3);
            var types = await db.ListingEvents.Select(e => e.Type).ToListAsync(TestContext.Current.CancellationToken);
            types.ShouldContain(ListingEventType.PriceChanged);
            types.ShouldContain(ListingEventType.StatusChanged);
            (await db.OutboxMessages.CountAsync(m => m.ProcessedAt == null && m.Attempts == 0, TestContext.Current.CancellationToken)).ShouldBe(0);
        }

        sent.ShouldHaveSingleItem().Body.ShouldContain("Precio");

        clock.Advance(TimeSpan.FromDays(1));
        adapter.Listings = [Observation("1", 229_000, "Piso en Inca")];
        await RunAsync(provider, sourceId);
        await using (var scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HomeRadarDbContext>();
            (await db.ListingSnapshots.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(3);
            var inca = await db.Listings.SingleAsync(l => l.ExternalId == "1", TestContext.Current.CancellationToken);
            inca.LastSeenAt.ShouldBe(T0.AddDays(4));
        }
    }

    [Fact]
    public async Task New_listings_are_analysed_and_affordability_searches_only_alert_what_the_buyer_can_pay()
    {
        var connection = await fixture.CreateDatabaseAsync("ingest");
        var adapter = new FakeAdapter();
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 11, 15, 8, 0, 0, TimeSpan.Zero));
        var sent = new List<Notification>();
        await using var provider = BuildServices(connection, adapter, clock, sent);

        Guid sourceId;
        await using (var scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HomeRadarDbContext>();
            var profile = await FinanceSeed.ApplyAsync(db);
            var source = new Source("fake", "Fake portal", SourceKind.Servicer, SellerType.Servicer, "fake", TimeSpan.FromHours(1), true, "{}");
            db.Sources.Add(source);
            db.SearchProfiles.Add(new SearchProfile("asequible", 1, "Lo que puedo pagar", new ListingFilter { Affordability = new AffordabilityCriteria { FinancingProfileId = profile.Id } }, [AlertChannel.Telegram], true, true));
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            sourceId = source.Id;
        }

        adapter.Listings = [Observation("1", 245_000, "Piso en Inca"), Observation("2", 420_000, "Casa en Pollença", "Pollença")];
        await RunAsync(provider, sourceId);
        await DispatchAsync(provider);

        await using (var scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HomeRadarDbContext>();
            var analyses = await db.ListingAnalyses.ToListAsync(TestContext.Current.CancellationToken);
            analyses.Count.ShouldBe(2);
            analyses.Select(analysis => analysis.Verdict).ShouldBe([AffordabilityVerdict.Affordable, AffordabilityVerdict.NotAffordable], ignoreOrder: true);
        }

        var alert = sent.ShouldHaveSingleItem();
        alert.Body.ShouldContain("Piso en Inca");
        alert.Body.ShouldContain("✅ Asequible con «Perfil de prueba»");
        alert.Body.ShouldContain("Tu precio máximo aquí");
    }

    [Fact]
    public async Task Empty_result_after_a_good_run_degrades_the_source_instead_of_withdrawing_everything()
    {
        var connection = await fixture.CreateDatabaseAsync("degrade");
        var adapter = new FakeAdapter();
        var clock = new FakeTimeProvider(T0);
        await using var provider = BuildServices(connection, adapter, clock, []);

        Guid sourceId;
        await using (var scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HomeRadarDbContext>();
            var source = new Source("fake", "Fake portal", SourceKind.Servicer, SellerType.Servicer, "fake", TimeSpan.FromHours(1), true, "{}");
            db.Sources.Add(source);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            sourceId = source.Id;
        }

        adapter.Listings = [Observation("1", 245_000, "Piso en Inca")];
        await RunAsync(provider, sourceId);
        adapter.Listings = [];
        var run = await RunAsync(provider, sourceId);

        run.Outcome.ShouldBe(SourceRunOutcome.Degraded);
        await using var verify = provider.CreateAsyncScope();
        var verifyDb = verify.ServiceProvider.GetRequiredService<HomeRadarDbContext>();
        (await verifyDb.Listings.SingleAsync(TestContext.Current.CancellationToken)).Status.ShouldBe(ListingStatus.Active);
        (await verifyDb.Sources.SingleAsync(TestContext.Current.CancellationToken)).Status.ShouldBe(SourceStatus.Degraded);
        (await verifyDb.OutboxMessages.CountAsync(m => m.Type == OutboxMessageTypes.SourceHealth, TestContext.Current.CancellationToken)).ShouldBe(1);
    }

    private static ServiceProvider BuildServices(string connection, FakeAdapter adapter, FakeTimeProvider clock, List<Notification> sent)
    {
        var services = new ServiceCollection();
        services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));
        services.AddHomeRadarPersistence(connection);
        services.AddHomeRadarApplication();
        services.AddSingleton<TimeProvider>(clock);
        services.AddSingleton<ISourceAdapterRegistry>(new FakeRegistry(adapter));
        services.AddSingleton<INotificationChannel>(new RecordingChannel(sent));
        return services.BuildServiceProvider();
    }

    private static async Task<SourceRun> RunAsync(ServiceProvider provider, Guid sourceId)
    {
        await using var scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<SourceIngestionService>().RunAsync(sourceId, fullScan: true, TestContext.Current.CancellationToken);
    }

    private static async Task DispatchAsync(ServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<OutboxDispatcher>().DispatchPendingAsync(TestContext.Current.CancellationToken);
    }

    private static ListingObservation Observation(string id, decimal price, string title, string municipality = "Inca") => new()
    {
        ExternalId = id,
        Url = new Uri($"https://fake.test/{id}"),
        Title = title,
        Price = Money.Euros(price),
        BuiltArea = new Area(95),
        Rooms = 3,
        Bathrooms = 1,
        PropertyType = PropertyType.Flat,
        Municipality = municipality,
        PhotoUrls = [new Uri($"https://fake.test/{id}/1.jpg")],
    };

    private sealed class FakeAdapter : IListingSource
    {
        public List<ListingObservation> Listings { get; set; } = [];

        public string AdapterKey => "fake";

        public async IAsyncEnumerable<FetchedListing> FetchAsync(SourceFetchContext context, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (var observation in Listings)
            {
                await Task.Yield();
                yield return FetchedListing.Of(observation, new RawDocument("text/html", $"<html>{observation.ExternalId}:{observation.Price}</html>"));
            }
        }
    }

    private sealed class FakeRegistry(IListingSource adapter) : ISourceAdapterRegistry
    {
        public IReadOnlyCollection<string> Keys => [adapter.AdapterKey];

        public IListingSource Resolve(string adapterKey) => adapter;
    }

    private sealed class RecordingChannel(List<Notification> sent) : INotificationChannel
    {
        public AlertChannel Channel => AlertChannel.Telegram;

        public Task SendAsync(Notification notification, CancellationToken cancellationToken)
        {
            sent.Add(notification);
            return Task.CompletedTask;
        }
    }
}
