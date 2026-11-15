using HomeRadar.Application.Ports;
using HomeRadar.Domain.Common;
using HomeRadar.Domain.Configuration;
using HomeRadar.Domain.Finance;
using HomeRadar.Domain.Intelligence;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.Outbox;
using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.Sources;
using HomeRadar.Domain.Watchlist;
using Microsoft.EntityFrameworkCore;

namespace HomeRadar.Infrastructure.Persistence;

public sealed class HomeRadarDbContext(DbContextOptions<HomeRadarDbContext> options) : DbContext(options), IHomeRadarDbContext
{
    public DbSet<Source> Sources => Set<Source>();

    public DbSet<SourceRun> SourceRuns => Set<SourceRun>();

    public DbSet<Agency> Agencies => Set<Agency>();

    public DbSet<Listing> Listings => Set<Listing>();

    public DbSet<Dwelling> Dwellings => Set<Dwelling>();

    public DbSet<ListingSnapshot> ListingSnapshots => Set<ListingSnapshot>();

    public DbSet<ListingEvent> ListingEvents => Set<ListingEvent>();

    public DbSet<SearchProfile> SearchProfiles => Set<SearchProfile>();

    public DbSet<FinancingProfile> FinancingProfiles => Set<FinancingProfile>();

    public DbSet<WatchlistItem> WatchlistItems => Set<WatchlistItem>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    public DbSet<BusinessConfigVersion> BusinessConfigVersions => Set<BusinessConfigVersion>();

    public DbSet<TaxRuleSet> TaxRuleSets => Set<TaxRuleSet>();

    public DbSet<ListingAnalysis> ListingAnalyses => Set<ListingAnalysis>();

    public DbSet<ListingRenovationOverride> RenovationOverrides => Set<ListingRenovationOverride>();

    public DbSet<CadastralParcel> CadastralParcels => Set<CadastralParcel>();

    public DbSet<ReferenceRate> ReferenceRates => Set<ReferenceRate>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        PersistDomainEvents();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HomeRadarDbContext).Assembly);
    }

    private void PersistDomainEvents()
    {
        var entities = ChangeTracker.Entries<Entity>().Where(entry => entry.Entity.DomainEvents.Count > 0).Select(entry => entry.Entity).ToList();
        foreach (var entity in entities)
        {
            foreach (var listingEvent in entity.DomainEvents.OfType<ListingEvent>())
            {
                ListingEvents.Add(listingEvent);
            }

            entity.ClearDomainEvents();
        }
    }
}
