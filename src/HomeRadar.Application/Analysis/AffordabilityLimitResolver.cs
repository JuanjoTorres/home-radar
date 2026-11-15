using HomeRadar.Application.Search;
using HomeRadar.Domain.Common;
using HomeRadar.Domain.Finance;
using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Application.Analysis;

public sealed class AffordabilityLimitResolver(AnalysisService analysis)
{
    private static readonly Money ReferencePrice = Money.Euros(100_000m);

    public async Task<Result<AffordablePriceLimits>> ResolveAsync(AffordabilityCriteria criteria, CancellationToken cancellationToken)
    {
        if (criteria.FinancingProfileId is null && criteria.Budget is null)
        {
            return AnalysisErrors.MissingCriteria();
        }

        var configuration = await analysis.ConfigurationAsync(null, cancellationToken);
        if (!configuration.IsSuccess)
        {
            return Result.Failure<AffordablePriceLimits>(configuration.Error!);
        }

        FinancingInputs? financing = null;
        if (criteria.FinancingProfileId is { } profileId)
        {
            var profile = await analysis.ResolveProfileAsync(profileId, cancellationToken);
            if (!profile.IsSuccess)
            {
                return Result.Failure<AffordablePriceLimits>(profile.Error!);
            }

            financing = FinancingInputs.From(profile.Value!);
        }

        var config = configuration.Value!;
        var limits = new List<SubdivisionPriceLimit>();
        foreach (var subdivision in config.Analyzer.Subdivisions)
        {
            var resale = Maximum(config, subdivision, PurchaseKind.Resale, criteria, financing);
            if (!resale.IsSuccess)
            {
                return Result.Failure<AffordablePriceLimits>(resale.Error!);
            }

            var newBuild = Maximum(config, subdivision, PurchaseKind.NewBuild, criteria, financing);
            limits.Add(new SubdivisionPriceLimit(subdivision.Code, subdivision.IsDefault, MunicipalityKeys(subdivision), resale.Value, newBuild.Value));
        }

        return new AffordablePriceLimits(limits);
    }

    public static IReadOnlyList<string> MunicipalityKeys(Subdivision subdivision) => subdivision.Municipalities
        .SelectMany(name => new[] { name.Trim().ToLowerInvariant(), TextKeys.Fold(name) })
        .Distinct(StringComparer.Ordinal)
        .ToList();

    private static Result<decimal?> Maximum(FinanceConfiguration config, Subdivision subdivision, PurchaseKind purchase, AffordabilityCriteria criteria, FinancingInputs? financing)
    {
        var municipality = subdivision.Municipalities.Count > 0 ? subdivision.Municipalities[0] : null;
        var scenario = new PurchaseScenario(ReferencePrice, purchase, PropertyType.Flat, HousingRegime.FreeMarket, municipality, null, Money.Zero, false, config.On);
        decimal? limit = null;
        if (financing is not null)
        {
            var affordable = config.Analyzer.MaximumAffordablePrice(scenario, financing, criteria.IncludeGuarantees, criteria.MinimumVerdict);
            if (!affordable.IsSuccess)
            {
                return Result.Failure<decimal?>(affordable.Error!);
            }

            limit = affordable.Value!.Price?.Amount ?? -1m;
        }

        if (criteria.Budget is { } budget)
        {
            var withinBudget = config.Analyzer.MaximumPriceWithinBudget(scenario, financing, Money.Euros(budget))?.Amount ?? -1m;
            limit = limit is { } current ? Math.Min(current, withinBudget) : withinBudget;
        }

        return Result.Success(limit);
    }
}
