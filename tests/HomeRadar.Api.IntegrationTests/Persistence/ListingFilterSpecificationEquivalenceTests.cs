using HomeRadar.Api.IntegrationTests.Support;
using HomeRadar.Application.Search;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.Sources;
using HomeRadar.Domain.ValueObjects;
using HomeRadar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace HomeRadar.Api.IntegrationTests.Persistence;

[Collection(DatabaseTests.Name)]
public sealed class ListingFilterSpecificationEquivalenceTests(DatabaseFixture fixture)
{
    private static readonly DateTimeOffset Now = new(2026, 11, 14, 12, 0, 0, TimeSpan.Zero);

    public static TheoryData<string, ListingFilter> Filters => new()
    {
        { "price range", new ListingFilter { PriceMin = 200_000, PriceMax = 260_000 } },
        { "rooms and municipality", new ListingFilter { RoomsMin = 3, Municipalities = ["inca", "Lloseta"] } },
        { "price per m2", new ListingFilter { PricePerM2Max = 2_600 } },
        { "radius around Inca", new ListingFilter { Radius = new GeoRadius(39.7211, 2.9108, 3_000) } },
        { "polygon around Palma", new ListingFilter { PolygonWkt = "POLYGON((2.55 39.50, 2.75 39.50, 2.75 39.65, 2.55 39.65, 2.55 39.50))" } },
        { "new within 24h", new ListingFilter { NewWithinHours = 24 } },
        { "days on market", new ListingFilter { DaysOnMarketMin = 10 } },
        { "price drops", new ListingFilter { PriceDropsMin = 1, CumulativeDropPercentMin = 5 } },
        { "possession and elevator", new ListingFilter { ExcludePossession = [PossessionStatus.NoPossession], Elevator = true } },
        { "seller and source", new ListingFilter { SellerTypes = [SellerType.Servicer], SourceKeys = ["solvia"] } },
        { "withdrawn included", new ListingFilter { Statuses = [ListingStatus.Active, ListingStatus.Withdrawn] } },
    };

    [Theory]
    [MemberData(nameof(Filters))]
    public async Task Database_and_in_memory_evaluation_agree(string name, ListingFilter filter)
    {
        name.ShouldNotBeNull();
        var connection = await fixture.CreateDatabaseAsync("spec");
        await using var db = new HomeRadarDbContext(DatabaseFixture.BuildOptions(connection));
        await SeedAsync(db);

        var expression = ListingFilterSpecification.Build(filter, Now);
        var fromDatabase = await db.Listings.AsNoTracking().Where(expression).Select(l => l.ExternalId).OrderBy(id => id).ToListAsync(TestContext.Current.CancellationToken);
        var all = await db.Listings.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken);
        var inMemory = all.Where(expression.Compile()).Select(l => l.ExternalId).OrderBy(id => id).ToList();

        inMemory.ShouldBe(fromDatabase);
        fromDatabase.ShouldNotBeEmpty();
        all.Count.ShouldBe(5);
    }

    private static async Task SeedAsync(HomeRadarDbContext db)
    {
        var solvia = new Source("solvia", "Solvia", SourceKind.Servicer, SellerType.Servicer, "solvia", TimeSpan.FromHours(6), true, "{}");
        var agency = new Source("inmovilla-mir", "Inmobiliaria Mir", SourceKind.Agency, SellerType.Agency, "inmovilla", TimeSpan.FromHours(2), false, "{}");
        db.Sources.AddRange(solvia, agency);

        db.Listings.Add(Make(solvia, "A", 245_000, 95, 3, "Inca", 39.7211, 2.9108, Now.AddDays(-20), elevator: true));
        db.Listings.Add(Make(solvia, "B", 199_000, 80, 2, "Lloseta", 39.7185, 2.8665, Now.AddHours(-3), possession: PossessionStatus.NoPossession));
        var dropped = Make(agency, "C", 300_000, 110, 4, "Palma", 39.5696, 2.6502, Now.AddDays(-40), elevator: false);
        dropped.Observe(Observation("C", 270_000, 110, 4, "Palma", 39.5696, 2.6502, elevator: false), Now.AddDays(-5));
        db.Listings.Add(dropped);
        var withdrawn = Make(agency, "D", 150_000, 60, 1, "Manacor", 39.5696, 3.2090, Now.AddDays(-90));
        withdrawn.MarkNotSeen(Now.AddDays(-1));
        db.Listings.Add(withdrawn);
        db.Listings.Add(Make(agency, "E", 520_000, 180, 4, "Pollença", 39.8770, 3.0160, Now.AddDays(-2), elevator: true));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static Listing Make(Source source, string id, decimal price, decimal m2, int rooms, string municipality, double lat, double lon, DateTimeOffset firstSeen, bool? elevator = null, PossessionStatus possession = PossessionStatus.Unknown) =>
        Listing.Create(source.Id, source.Key, source.SellerType, Observation(id, price, m2, rooms, municipality, lat, lon, elevator, possession), firstSeen);

    private static ListingObservation Observation(string id, decimal price, decimal m2, int rooms, string municipality, double lat, double lon, bool? elevator = null, PossessionStatus possession = PossessionStatus.Unknown)
    {
        var features = new Dictionary<string, string>(StringComparer.Ordinal);
        if (elevator is { } flag)
        {
            features[FeatureKeys.Elevator] = flag ? "true" : "false";
        }

        return new ListingObservation
        {
            ExternalId = id,
            Url = new Uri($"https://example.test/{id}"),
            Title = $"Listing {id}",
            Price = Money.Euros(price),
            BuiltArea = new Area(m2),
            Rooms = rooms,
            Bathrooms = 1,
            PropertyType = PropertyType.Flat,
            Municipality = municipality,
            Location = new Point(lon, lat) { SRID = UtmProjection.Wgs84Srid },
            Features = features,
            Possession = possession,
        };
    }
}
