using HomeRadar.Application.Ports;
using HomeRadar.Domain.Common;
using HomeRadar.Domain.Configuration;
using HomeRadar.Domain.Finance;
using Microsoft.EntityFrameworkCore;

namespace HomeRadar.Application.Analysis;

public sealed record FinanceConfiguration(DateOnly On, BusinessConfigVersion Version, IReadOnlyList<TaxRuleSet> TaxRuleSets, PurchaseAnalyzer Analyzer);

public sealed class FinanceConfigurationLoader(IHomeRadarDbContext db)
{
    private readonly Dictionary<DateOnly, FinanceConfiguration> _cache = [];

    public async Task<Result<FinanceConfiguration>> LoadAsync(DateOnly on, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(on, out var cached))
        {
            return cached;
        }

        var version = await db.BusinessConfigVersions.AsNoTracking()
            .Where(candidate => candidate.ValidFrom <= on)
            .OrderByDescending(candidate => candidate.ValidFrom)
            .ThenByDescending(candidate => candidate.Revision)
            .FirstOrDefaultAsync(cancellationToken);
        if (version is null)
        {
            return AnalysisErrors.NoBusinessConfig(on);
        }

        var chain = Jurisdictions.Chain(version.Config.DefaultJurisdiction);
        var candidates = await db.TaxRuleSets.AsNoTracking()
            .Where(set => chain.Contains(set.Jurisdiction) && set.ValidFrom <= on)
            .ToListAsync(cancellationToken);
        var sets = chain
            .Select(code => candidates.Where(set => set.Jurisdiction == code).OrderByDescending(set => set.ValidFrom).ThenByDescending(set => set.Revision).FirstOrDefault())
            .OfType<TaxRuleSet>()
            .ToList();
        if (sets.Count == 0)
        {
            return AnalysisErrors.NoTaxRules(version.Config.DefaultJurisdiction, on);
        }

        var configuration = new FinanceConfiguration(on, version, sets, new PurchaseAnalyzer(version.Config, sets.Select(set => set.Document).ToList()));
        _cache[on] = configuration;
        return configuration;
    }
}

public static class AnalysisErrors
{
    public static DomainError NoBusinessConfig(DateOnly on) => new("finance.noBusinessConfig", $"No business configuration is valid on {on:yyyy-MM-dd}; check config/seed/business-config.yaml.");

    public static DomainError NoTaxRules(string jurisdiction, DateOnly on) => new("finance.noTaxRules", $"No tax rule set for {jurisdiction} is valid on {on:yyyy-MM-dd}; check config/seed/tax-rules.");

    public static DomainError ListingNotFound(Guid id) => new("listing.notFound", $"Listing {id} does not exist.");

    public static DomainError ListingWithoutPrice(Guid id) => new("listing.noPrice", $"Listing {id} has no published price to analyse.");

    public static DomainError ProfileNotFound(Guid id) => new("financingProfile.notFound", $"Financing profile {id} does not exist.");

    public static DomainError NoDefaultProfile() => new("financingProfile.noDefault", "No financing profile is marked as default; pass financingProfileId.");

    public static DomainError MissingCriteria() => new("affordability.criteria", "Affordability needs a financingProfileId, a budget or both.");
}
