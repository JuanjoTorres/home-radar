using HomeRadar.Domain.Configuration;
using HomeRadar.Domain.Finance;
using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.ValueObjects;
using HomeRadar.Infrastructure.Persistence;
using HomeRadar.Infrastructure.Seeding;

namespace HomeRadar.Api.IntegrationTests.Support;

public static class FinanceSeed
{
    private static readonly DateTimeOffset LoadedAt = new(2026, 11, 15, 0, 0, 0, TimeSpan.Zero);

    public static string Directory => Path.Combine(AppContext.BaseDirectory, "seed");

    public static async Task<FinancingProfile> ApplyAsync(HomeRadarDbContext db, decimal savings = 60_000m)
    {
        foreach (var seed in YamlSeedLoader.LoadTaxRuleSets(Directory))
        {
            db.TaxRuleSets.Add(new TaxRuleSet(seed.Document, seed.ValidFrom, seed.Revision, seed.VerifiedOn, seed.SourceFile, LoadedAt));
        }

        foreach (var seed in YamlSeedLoader.LoadBusinessConfigs(Directory))
        {
            db.BusinessConfigVersions.Add(new BusinessConfigVersion(seed.ValidFrom, seed.Revision, seed.Config, seed.Source, LoadedAt));
        }

        var profile = new FinancingProfile(
            "default",
            1,
            "Perfil de prueba",
            Money.Euros(savings),
            Money.Euros(3_000m),
            Money.Zero,
            0.35m,
            new MortgageTerms(MortgageType.Fixed, 0.8m, 2.9m, null, null, 360),
            new BuyerProfile(34, FirstHome: true, PrimaryResidence: true, ResidentInBalearics: true, 5, LargeFamily: false, SingleParentFamily: false, null, null, null),
            isDefault: true);
        db.FinancingProfiles.Add(profile);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return profile;
    }
}
