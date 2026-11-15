using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HomeRadar.Application.Ports;
using HomeRadar.Domain.Common;
using HomeRadar.Domain.Finance;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace HomeRadar.Application.Analysis;

public sealed record TaxRuleSetReference(Guid Id, string Jurisdiction, DateOnly ValidFrom, int Revision);

public sealed record ListingAnalysisInput(
    Guid ListingId,
    string ListingContentHash,
    decimal Price,
    PurchaseKind Purchase,
    PropertyType PropertyType,
    string? Municipality,
    Guid FinancingProfileId,
    uint FinancingProfileVersion,
    FinancingInputs Financing,
    Guid BusinessConfigVersionId,
    int BusinessConfigRevision,
    IReadOnlyList<TaxRuleSetReference> TaxRuleSets,
    DateOnly On);

public sealed record StoredListingAnalysis(ListingAnalysis Record, FinancialAnalysis Analysis, ListingAnalysisInput Input, bool Reused);

public sealed class AnalysisService(IHomeRadarDbContext db, FinanceConfigurationLoader loader, TimeProvider clock)
{
    public DateOnly Today => DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);

    public Task<Result<FinanceConfiguration>> ConfigurationAsync(DateOnly? on, CancellationToken cancellationToken) =>
        loader.LoadAsync(on ?? Today, cancellationToken);

    public async Task<Result<FinancingProfile>> ResolveProfileAsync(Guid? financingProfileId, CancellationToken cancellationToken)
    {
        if (financingProfileId is { } id)
        {
            var profile = await db.FinancingProfiles.AsNoTracking().SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
            return profile is null ? AnalysisErrors.ProfileNotFound(id) : profile;
        }

        var fallback = await db.FinancingProfiles.AsNoTracking()
            .Where(candidate => candidate.IsDefault)
            .OrderByDescending(candidate => candidate.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        return fallback is null ? AnalysisErrors.NoDefaultProfile() : fallback;
    }

    public async Task<Result<StoredListingAnalysis>> AnalyzeListingAsync(Guid listingId, Guid? financingProfileId, CancellationToken cancellationToken)
    {
        var listing = await db.Listings.AsNoTracking().SingleOrDefaultAsync(candidate => candidate.Id == listingId, cancellationToken);
        if (listing is null)
        {
            return AnalysisErrors.ListingNotFound(listingId);
        }

        var profile = await ResolveProfileAsync(financingProfileId, cancellationToken);
        if (!profile.IsSuccess)
        {
            return Result.Failure<StoredListingAnalysis>(profile.Error!);
        }

        var stored = await AnalyzeAsync(listing, profile.Value!, cancellationToken);
        if (stored.IsSuccess && !stored.Value!.Reused)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return stored;
    }

    public async Task<Result<StoredListingAnalysis>> AnalyzeAsync(Listing listing, FinancingProfile profile, CancellationToken cancellationToken)
    {
        if (listing.Price is not { } price)
        {
            return AnalysisErrors.ListingWithoutPrice(listing.Id);
        }

        var configuration = await loader.LoadAsync(Today, cancellationToken);
        if (!configuration.IsSuccess)
        {
            return Result.Failure<StoredListingAnalysis>(configuration.Error!);
        }

        var config = configuration.Value!;
        var input = new ListingAnalysisInput(
            listing.Id,
            listing.ContentHash.Value,
            price.Amount,
            listing.IsNewBuild ? PurchaseKind.NewBuild : PurchaseKind.Resale,
            listing.PropertyType,
            listing.Municipality,
            profile.Id,
            profile.Version,
            FinancingInputs.From(profile),
            config.Version.Id,
            config.Version.Revision,
            config.TaxRuleSets.Select(set => new TaxRuleSetReference(set.Id, set.Jurisdiction, set.ValidFrom, set.Revision)).ToList(),
            config.On);
        var inputJson = JsonSerializer.Serialize(input, FinanceJson.Options);
        var inputHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(inputJson)));

        var existing = await db.ListingAnalyses.AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.ListingId == listing.Id && candidate.FinancingProfileId == profile.Id && candidate.InputHash == inputHash, cancellationToken);
        if (existing is not null)
        {
            return new StoredListingAnalysis(existing, JsonSerializer.Deserialize<FinancialAnalysis>(existing.ResultJson, FinanceJson.Options)!, input, Reused: true);
        }

        var scenario = new PurchaseScenario(price, input.Purchase, listing.PropertyType, HousingRegime.FreeMarket, listing.Municipality, null, Money.Zero, false, config.On);
        var analysis = config.Analyzer.Analyze(scenario, input.Financing);
        if (!analysis.IsSuccess)
        {
            return Result.Failure<StoredListingAnalysis>(analysis.Error!);
        }

        var record = new ListingAnalysis(
            listing.Id,
            profile.Id,
            inputHash,
            config.Version.Id,
            config.TaxRuleSets.Select(set => set.Id).ToList(),
            analysis.Value!.Base.Verdict,
            price.Amount,
            analysis.Value.MaximumPrice?.Price?.Amount,
            inputJson,
            JsonSerializer.Serialize(analysis.Value, FinanceJson.Options),
            clock.GetUtcNow());
        db.ListingAnalyses.Add(record);
        return new StoredListingAnalysis(record, analysis.Value, input, Reused: false);
    }
}
