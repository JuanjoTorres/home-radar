using HomeRadar.Domain.Configuration;
using HomeRadar.Domain.Finance;
using HomeRadar.Domain.Intelligence;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.Outbox;
using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.Sources;
using HomeRadar.Domain.Watchlist;
using Microsoft.EntityFrameworkCore;

namespace HomeRadar.Application.Ports;

/// <summary>
/// Unit of work over the HomeRadar database. Commands track entities; read models query with <c>AsNoTracking</c>.
/// Saving changes also persists the <see cref="ListingEvent"/>s raised by aggregates in the same transaction.
/// </summary>
public interface IHomeRadarDbContext
{
    DbSet<Source> Sources { get; }

    DbSet<SourceRun> SourceRuns { get; }

    DbSet<Agency> Agencies { get; }

    DbSet<Listing> Listings { get; }

    DbSet<Dwelling> Dwellings { get; }

    DbSet<ListingSnapshot> ListingSnapshots { get; }

    DbSet<ListingEvent> ListingEvents { get; }

    DbSet<SearchProfile> SearchProfiles { get; }

    DbSet<FinancingProfile> FinancingProfiles { get; }

    DbSet<WatchlistItem> WatchlistItems { get; }

    DbSet<OutboxMessage> OutboxMessages { get; }

    DbSet<IdempotencyRecord> IdempotencyRecords { get; }

    DbSet<BusinessConfigVersion> BusinessConfigVersions { get; }

    DbSet<TaxRuleSet> TaxRuleSets { get; }

    DbSet<ListingAnalysis> ListingAnalyses { get; }

    DbSet<ListingRenovationOverride> RenovationOverrides { get; }

    DbSet<CadastralParcel> CadastralParcels { get; }

    DbSet<ReferenceRate> ReferenceRates { get; }

    /// <summary>Persists tracked changes and the domain events raised since the last save, atomically.</summary>
    /// <returns>The number of state entries written.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
