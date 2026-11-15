using HomeRadar.Application.Ports;
using HomeRadar.Domain.Configuration;
using HomeRadar.Domain.Finance;
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

    public async Task<FinanceSeedResult> ApplyFinanceAsync(IReadOnlyList<TaxRuleSetSeed> taxRuleSets, IReadOnlyList<BusinessConfigSeed> businessConfigs, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var existingSets = await db.TaxRuleSets.AsNoTracking()
            .Select(set => new { set.Jurisdiction, set.ValidFrom, set.Revision })
            .ToListAsync(cancellationToken);
        var added = 0;
        var rejected = 0;
        foreach (var seed in taxRuleSets)
        {
            var errors = TaxRuleDocumentValidator.Validate(seed.Document);
            if (errors.Count > 0)
            {
                rejected++;
                logger.LogError("Tax rule file {File} rejected: {Errors}", seed.SourceFile, string.Join("; ", errors));
                continue;
            }

            if (existingSets.Any(set => set.Jurisdiction == seed.Document.Jurisdiction && set.ValidFrom == seed.ValidFrom && set.Revision == seed.Revision))
            {
                continue;
            }

            db.TaxRuleSets.Add(new TaxRuleSet(seed.Document, seed.ValidFrom, seed.Revision, seed.VerifiedOn, seed.SourceFile, now));
            added++;
        }

        var existingConfigs = await db.BusinessConfigVersions.AsNoTracking()
            .Select(version => new { version.ValidFrom, version.Revision })
            .ToListAsync(cancellationToken);
        foreach (var seed in businessConfigs.Where(seed => !existingConfigs.Any(version => version.ValidFrom == seed.ValidFrom && version.Revision == seed.Revision)))
        {
            db.BusinessConfigVersions.Add(new BusinessConfigVersion(seed.ValidFrom, seed.Revision, seed.Config, seed.Source, now));
            added++;
        }

        await db.SaveChangesAsync(cancellationToken);
        return new FinanceSeedResult(added, rejected);
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

public sealed record FinanceSeedResult(int Added, int Rejected);
