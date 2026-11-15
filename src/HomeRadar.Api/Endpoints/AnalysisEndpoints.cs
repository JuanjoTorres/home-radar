using HomeRadar.Api.Contracts;
using HomeRadar.Api.Infrastructure;
using HomeRadar.Application.Analysis;
using HomeRadar.Application.Ports;
using HomeRadar.Domain.Common;
using HomeRadar.Domain.Finance;
using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeRadar.Api.Endpoints;

public static class AnalysisEndpoints
{
    private const string Tag = "Analysis";
    private const string ConfigurationTag = "Configuration";

    public static RouteGroupBuilder MapAnalysisEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/listings/{listingId:guid}/analysis", GetListingAnalysisAsync).Document("getListingAnalysis", "Analyse whether a listing is affordable with a financing profile.", Tag)
            .Produces<ListingAnalysisDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/analysis/simulations", SimulateAsync).Document("simulatePurchase", "Simulate a purchase without a listing.", Tag)
            .Produces<SimulationResultDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/affordability/maximum-price", MaximumPriceAsync).Document("calculateMaximumAffordablePrice", "Highest price a financing profile can pay.", Tag)
            .Produces<MaximumPriceResultDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/business-config", GetBusinessConfigAsync).Document("getBusinessConfig", "Business configuration valid on a date.", ConfigurationTag)
            .Produces<BusinessConfigDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/business-config/versions", ListBusinessConfigVersionsAsync).Document("listBusinessConfigVersions", "Every business configuration version.", ConfigurationTag)
            .Produces<IReadOnlyList<BusinessConfigDto>>();

        group.MapGet("/tax-rules", ListTaxRulesAsync).Document("listTaxRules", "Tax rule sets applied on a date.", ConfigurationTag)
            .Produces<IReadOnlyList<TaxRuleSetDto>>()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return group;
    }

    private static async Task<Results<Ok<ListingAnalysisDto>, ProblemHttpResult>> GetListingAnalysisAsync(Guid listingId, [FromQuery] Guid? financingProfileId, AnalysisService analysis, IHomeRadarDbContext db, CancellationToken cancellationToken)
    {
        var result = await analysis.AnalyzeListingAsync(listingId, financingProfileId, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problems.From(result.Error!);
        }

        var stored = result.Value!;
        var validFrom = await db.BusinessConfigVersions.AsNoTracking().Where(version => version.Id == stored.Input.BusinessConfigVersionId).Select(version => version.ValidFrom).SingleAsync(cancellationToken);
        return TypedResults.Ok(new ListingAnalysisDto
        {
            AnalysisId = stored.Record.Id,
            ListingId = stored.Record.ListingId,
            FinancingProfileId = stored.Record.FinancingProfileId,
            CalculatedAt = stored.Record.CalculatedAt,
            Reused = stored.Reused,
            Configuration = AnalysisConfigurationDto.From(stored.Input, validFrom),
            Result = FinancialAnalysisDto.From(stored.Analysis),
        });
    }

    private static async Task<Results<Ok<SimulationResultDto>, ValidationProblem, ProblemHttpResult>> SimulateAsync(SimulationRequestDto request, AnalysisService analysis, CancellationToken cancellationToken)
    {
        var errors = ValidateSimulation(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        var configuration = await analysis.ConfigurationAsync(request.On, cancellationToken);
        if (!configuration.IsSuccess)
        {
            return Problems.From(configuration.Error!);
        }

        var financing = await FinancingAsync(request.Financing, request.FinancingProfileId, analysis, cancellationToken);
        if (!financing.IsSuccess)
        {
            return Problems.From(financing.Error!);
        }

        var config = configuration.Value!;
        var analyzer = request.EuriborPercent is { } euribor
            ? new PurchaseAnalyzer(config.Version.Config with { Euribor = config.Version.Config.Euribor with { TwelveMonthPercent = euribor, Month = "override", SourceUrl = null } }, config.TaxRuleSets.Select(set => set.Document).ToList())
            : config.Analyzer;
        var scenario = new PurchaseScenario(
            Money.Euros(request.Price.Amount),
            Enum.Parse<PurchaseKind>(request.PurchaseKind, ignoreCase: true),
            Enum.Parse<PropertyType>(request.PropertyType, ignoreCase: true),
            Enum.Parse<HousingRegime>(request.HousingRegime, ignoreCase: true),
            request.Municipality,
            request.CadastralReferenceValue is { } reference ? Money.Euros(reference.Amount) : null,
            Money.Euros(request.RenovationCost?.Amount ?? 0m),
            false,
            config.On);

        var result = analyzer.Analyze(scenario, financing.Value!);
        if (!result.IsSuccess)
        {
            return Problems.From(result.Error!);
        }

        return TypedResults.Ok(new SimulationResultDto
        {
            Configuration = AnalysisConfigurationDto.From(config),
            EuriborPercent = request.EuriborPercent ?? config.Version.Config.Euribor.TwelveMonthPercent,
            Result = FinancialAnalysisDto.From(result.Value!),
        });
    }

    private static async Task<Results<Ok<MaximumPriceResultDto>, ValidationProblem, ProblemHttpResult>> MaximumPriceAsync(MaximumPriceRequestDto request, AnalysisService analysis, CancellationToken cancellationToken)
    {
        var errors = ValidateMaximumPrice(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        var configuration = await analysis.ConfigurationAsync(request.On, cancellationToken);
        if (!configuration.IsSuccess)
        {
            return Problems.From(configuration.Error!);
        }

        var financing = await FinancingAsync(request.Financing, request.FinancingProfileId, analysis, cancellationToken);
        if (!financing.IsSuccess)
        {
            return Problems.From(financing.Error!);
        }

        var config = configuration.Value!;
        var scenario = new PurchaseScenario(
            Money.Euros(100_000m),
            Enum.Parse<PurchaseKind>(request.PurchaseKind, ignoreCase: true),
            Enum.Parse<PropertyType>(request.PropertyType, ignoreCase: true),
            HousingRegime.FreeMarket,
            request.Municipality,
            null,
            Money.Euros(request.RenovationCost?.Amount ?? 0m),
            false,
            config.On);
        var target = Enum.Parse<AffordabilityVerdict>(request.MinimumVerdict, ignoreCase: true);
        var maximum = config.Analyzer.MaximumAffordablePrice(scenario, financing.Value!, request.IncludeGuarantees, target);
        if (!maximum.IsSuccess)
        {
            return Problems.From(maximum.Error!);
        }

        PurchaseAnalysisDto? atMaximum = null;
        if (maximum.Value!.Price is { } price)
        {
            var analyzed = config.Analyzer.Analyze(scenario.WithPrice(price), financing.Value!, includeMaximumPrice: false).Value!;
            var chosen = request.IncludeGuarantees && analyzed.WithGuarantee is { } guaranteed && guaranteed.Verdict <= analyzed.Base.Verdict ? guaranteed : analyzed.Base;
            atMaximum = PurchaseAnalysisDto.From(chosen);
        }

        return TypedResults.Ok(new MaximumPriceResultDto
        {
            Configuration = AnalysisConfigurationDto.From(config),
            Maximum = MaximumPriceDto.From(maximum.Value),
            AtMaximum = atMaximum,
        });
    }

    private static async Task<Results<Ok<BusinessConfigDto>, ProblemHttpResult>> GetBusinessConfigAsync([FromQuery] DateOnly? on, AnalysisService analysis, IHomeRadarDbContext db, CancellationToken cancellationToken)
    {
        var date = on ?? analysis.Today;
        var version = await db.BusinessConfigVersions.AsNoTracking()
            .Where(candidate => candidate.ValidFrom <= date)
            .OrderByDescending(candidate => candidate.ValidFrom)
            .ThenByDescending(candidate => candidate.Revision)
            .FirstOrDefaultAsync(cancellationToken);
        return version is null
            ? TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, title: "No business configuration", detail: $"No version is valid on {date:yyyy-MM-dd}.", type: ProblemTypes.NotFound)
            : TypedResults.Ok(BusinessConfigDto.From(version));
    }

    private static async Task<Ok<IReadOnlyList<BusinessConfigDto>>> ListBusinessConfigVersionsAsync(IHomeRadarDbContext db, CancellationToken cancellationToken)
    {
        var versions = await db.BusinessConfigVersions.AsNoTracking().OrderByDescending(version => version.ValidFrom).ThenByDescending(version => version.Revision).ToListAsync(cancellationToken);
        return TypedResults.Ok<IReadOnlyList<BusinessConfigDto>>(versions.Select(BusinessConfigDto.From).ToList());
    }

    private static async Task<Results<Ok<IReadOnlyList<TaxRuleSetDto>>, ProblemHttpResult>> ListTaxRulesAsync([FromQuery] string? jurisdiction, [FromQuery] DateOnly? on, AnalysisService analysis, IHomeRadarDbContext db, CancellationToken cancellationToken)
    {
        var date = on ?? analysis.Today;
        var configuration = await analysis.ConfigurationAsync(date, cancellationToken);
        if (!configuration.IsSuccess)
        {
            return Problems.From(configuration.Error!);
        }

        var chain = Jurisdictions.Chain(jurisdiction ?? configuration.Value!.Version.Config.DefaultJurisdiction);
        var candidates = await db.TaxRuleSets.AsNoTracking().Where(set => chain.Contains(set.Jurisdiction) && set.ValidFrom <= date).ToListAsync(cancellationToken);
        var effective = chain
            .Select(code => candidates.Where(set => set.Jurisdiction == code).OrderByDescending(set => set.ValidFrom).ThenByDescending(set => set.Revision).FirstOrDefault())
            .OfType<TaxRuleSet>()
            .Select(TaxRuleSetDto.From)
            .ToList();
        return TypedResults.Ok<IReadOnlyList<TaxRuleSetDto>>(effective);
    }

    private static async Task<Result<FinancingInputs>> FinancingAsync(FinancingAssumptionsDto? inline, Guid? profileId, AnalysisService analysis, CancellationToken cancellationToken)
    {
        if (inline is not null)
        {
            return new FinancingInputs(Money.Euros(inline.Savings.Amount), Money.Euros(inline.NetMonthlyIncome.Amount), Money.Euros(inline.ExistingMonthlyDebt.Amount), inline.MaxDebtToIncomeRatio, inline.Mortgage.ToDomain(), inline.Buyer.ToDomain());
        }

        var profile = await analysis.ResolveProfileAsync(profileId, cancellationToken);
        return profile.IsSuccess ? FinancingInputs.From(profile.Value!) : Result.Failure<FinancingInputs>(profile.Error!);
    }

    private static Dictionary<string, string[]> ValidateSimulation(SimulationRequestDto request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request.Price.Amount is <= 0m or > 50_000_000m)
        {
            errors["price"] = ["Must be between 0 and 50,000,000."];
        }

        ValidateEnums(errors, request.PurchaseKind, request.PropertyType);
        if (!Enum.TryParse<HousingRegime>(request.HousingRegime, ignoreCase: true, out _))
        {
            errors["housingRegime"] = ["Must be FreeMarket, PriceLimited, ProtectedSpecialRegime or AppraisedPrice."];
        }

        if (request.CadastralReferenceValue is { Amount: < 0m } || request.RenovationCost is { Amount: < 0m })
        {
            errors["amounts"] = ["cadastralReferenceValue and renovationCost cannot be negative."];
        }

        if (request.EuriborPercent is < -1m or > 15m)
        {
            errors["euriborPercent"] = ["Must be between -1 and 15."];
        }

        ValidateFinancing(errors, request.Financing);
        return errors;
    }

    private static Dictionary<string, string[]> ValidateMaximumPrice(MaximumPriceRequestDto request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        ValidateEnums(errors, request.PurchaseKind, request.PropertyType);
        if (!Enum.TryParse<AffordabilityVerdict>(request.MinimumVerdict, ignoreCase: true, out var verdict) || verdict == AffordabilityVerdict.NotAffordable)
        {
            errors["minimumVerdict"] = ["Must be Affordable or Borderline."];
        }

        if (request.RenovationCost is { Amount: < 0m })
        {
            errors["renovationCost"] = ["Cannot be negative."];
        }

        ValidateFinancing(errors, request.Financing);
        return errors;
    }

    private static void ValidateEnums(Dictionary<string, string[]> errors, string purchaseKind, string propertyType)
    {
        if (!Enum.TryParse<PurchaseKind>(purchaseKind, ignoreCase: true, out _))
        {
            errors["purchaseKind"] = ["Must be Resale or NewBuild."];
        }

        if (!Enum.TryParse<PropertyType>(propertyType, ignoreCase: true, out _))
        {
            errors["propertyType"] = ["Unknown property type."];
        }
    }

    private static void ValidateFinancing(Dictionary<string, string[]> errors, FinancingAssumptionsDto? financing)
    {
        if (financing is null)
        {
            return;
        }

        if (financing.Savings.Amount < 0 || financing.NetMonthlyIncome.Amount <= 0 || financing.ExistingMonthlyDebt.Amount < 0)
        {
            errors["financing.amounts"] = ["Savings and debt must be ≥ 0; income must be > 0."];
        }

        if (financing.MaxDebtToIncomeRatio is < 0.1m or > 0.6m)
        {
            errors["financing.maxDebtToIncomeRatio"] = ["Must be between 0.1 and 0.6."];
        }

        if (!Enum.TryParse<MortgageType>(financing.Mortgage.Type, ignoreCase: true, out _))
        {
            errors["financing.mortgage.type"] = ["Must be Fixed, Variable or Mixed."];
        }

        if (financing.Mortgage.MaxLoanToValue is < 0m or > 1m || financing.Mortgage.TermMonths is < 60 or > 480)
        {
            errors["financing.mortgage"] = ["maxLoanToValue must be 0–1 and termMonths 60–480."];
        }

        if (financing.Buyer.Age is < 18 or > 99 || financing.Buyer.BuyerCount is < 1 or > 2)
        {
            errors["financing.buyer"] = ["age must be 18–99 and buyerCount 1–2."];
        }
    }
}
