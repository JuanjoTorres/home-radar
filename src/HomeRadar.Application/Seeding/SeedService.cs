using HomeRadar.Application.Ports;
using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.Sources;
using HomeRadar.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeRadar.Application.Seeding;

public sealed class SeedService(IHomeRadarDbContext db, ISourceAdapterRegistry adapters, TimeProvider clock, ILogger<SeedService> logger)
{
    public async Task ApplyAsync(IReadOnlyList<SourceSeed> sources, IReadOnlyList<FinancingProfileSeed> financingProfiles, IReadOnlyList<SearchProfileSeed> searchProfiles, CancellationToken cancellationToken)
    {
        await ApplySourcesAsync(sources, cancellationToken);
        await ApplyFinancingProfilesAsync(financingProfiles, cancellationToken);
        await ApplySearchProfilesAsync(searchProfiles, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplySourcesAsync(IReadOnlyList<SourceSeed> seeds, CancellationToken cancellationToken)
    {
        var existing = await db.Sources.ToDictionaryAsync(source => source.Key, StringComparer.Ordinal, cancellationToken);
        foreach (var seed in seeds)
        {
            if (!adapters.Keys.Contains(seed.AdapterKey))
            {
                logger.LogWarning("Seed source {Key} references unknown adapter {Adapter}; skipped", seed.Key, seed.AdapterKey);
                continue;
            }

            if (existing.TryGetValue(seed.Key, out var source))
            {
                source.Configure(seed.PollInterval, seed.PriorityPollInterval, seed.Priority, seed.Enabled, seed.ConfigurationJson);
            }
            else
            {
                db.Sources.Add(new Source(seed.Key, seed.Name, seed.Kind, seed.SellerType, seed.AdapterKey, seed.PollInterval, seed.Priority, seed.ConfigurationJson));
            }
        }
    }

    private async Task ApplyFinancingProfilesAsync(IReadOnlyList<FinancingProfileSeed> seeds, CancellationToken cancellationToken)
    {
        var existing = await db.FinancingProfiles.ToDictionaryAsync(profile => profile.Key, StringComparer.Ordinal, cancellationToken);
        var now = clock.GetUtcNow();
        foreach (var seed in seeds)
        {
            if (existing.TryGetValue(seed.Key, out var profile))
            {
                if (seed.Revision > profile.Revision)
                {
                    profile.Update(seed.Revision, seed.Name, Money.Euros(seed.Savings), Money.Euros(seed.NetMonthlyIncome), Money.Euros(seed.ExistingMonthlyDebt), seed.MaxDebtToIncomeRatio, seed.Mortgage, seed.Buyer, seed.IsDefault, now);
                }
            }
            else
            {
                db.FinancingProfiles.Add(new FinancingProfile(seed.Key, seed.Revision, seed.Name, Money.Euros(seed.Savings), Money.Euros(seed.NetMonthlyIncome), Money.Euros(seed.ExistingMonthlyDebt), seed.MaxDebtToIncomeRatio, seed.Mortgage, seed.Buyer, seed.IsDefault));
            }
        }
    }

    private async Task ApplySearchProfilesAsync(IReadOnlyList<SearchProfileSeed> seeds, CancellationToken cancellationToken)
    {
        var existing = await db.SearchProfiles.ToDictionaryAsync(profile => profile.Key, StringComparer.Ordinal, cancellationToken);
        var now = clock.GetUtcNow();
        foreach (var seed in seeds)
        {
            if (existing.TryGetValue(seed.Key, out var profile))
            {
                if (seed.Revision > profile.Revision)
                {
                    profile.Update(seed.Revision, seed.Name, seed.Filter, seed.Channels, seed.Priority, seed.Enabled, now);
                }
            }
            else
            {
                db.SearchProfiles.Add(new SearchProfile(seed.Key, seed.Revision, seed.Name, seed.Filter, seed.Channels, seed.Priority, seed.Enabled));
            }
        }
    }
}
