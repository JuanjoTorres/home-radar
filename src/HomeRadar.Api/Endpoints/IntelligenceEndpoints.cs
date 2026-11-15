using HomeRadar.Api.Contracts;
using HomeRadar.Api.Infrastructure;
using HomeRadar.Application.Analysis;
using HomeRadar.Application.Intelligence;
using HomeRadar.Application.Ports;
using HomeRadar.Domain.Intelligence;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace HomeRadar.Api.Endpoints;

public static class IntelligenceEndpoints
{
    private const string Tag = "Intelligence";

    public static RouteGroupBuilder MapIntelligenceEndpoints(this RouteGroupBuilder group)
    {
        group.MapPut("/listings/{listingId:guid}/renovation-override", SetOverrideAsync).Document("setRenovationOverride", "Set the renovation of a listing by hand.", Tag)
            .Produces<RenovationEstimateDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status412PreconditionFailed)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapDelete("/listings/{listingId:guid}/renovation-override", DeleteOverrideAsync).Document("deleteRenovationOverride", "Go back to the rule-based renovation estimate.", Tag)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/listings/{listingId:guid}/cadastral-data", SetCadastralDataAsync).Document("setCadastralData", "Set Catastro data and the reference value you checked yourself.", Tag)
            .Produces<CadastralDataDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/renovation/estimates", EstimateAsync).Document("estimateRenovation", "Estimate a renovation for a listing or for manual parameters.", Tag)
            .Produces<RenovationEstimateDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return group;
    }

    private static async Task<IResult> SetOverrideAsync(
        Guid listingId,
        RenovationOverrideRequestDto request,
        HttpContext http,
        IHomeRadarDbContext db,
        AnalysisService analysis,
        ListingInsightService insights,
        TimeProvider clock,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        RenovationLevel? level = null;
        if (request.Level is { } levelText)
        {
            if (Enum.TryParse<RenovationLevel>(levelText, ignoreCase: true, out var parsed))
            {
                level = parsed;
            }
            else
            {
                errors["level"] = ["Must be ReadyToMoveIn, Light, Medium or Full."];
            }
        }

        if (level is null && request.Items.Count == 0 && request.TotalBudget is null && errors.Count == 0)
        {
            errors["override"] = ["Give a level, items or a total budget."];
        }

        if (request.Items.Any(item => item.Amount.Amount < 0m || string.IsNullOrWhiteSpace(item.Code)) || request.TotalBudget is { Amount: < 0m })
        {
            errors["items"] = ["Items need a code and non-negative amounts; the budget cannot be negative."];
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        var listing = await db.Listings.SingleOrDefaultAsync(candidate => candidate.Id == listingId, cancellationToken);
        if (listing is null)
        {
            return Problems.From(AnalysisErrors.ListingNotFound(listingId));
        }

        var items = request.Items.Select(item => new RenovationOverrideItem(item.Code, item.Description, item.Amount.Amount)).ToList();
        var existing = await db.RenovationOverrides.SingleOrDefaultAsync(candidate => candidate.ListingId == listingId, cancellationToken);
        if (existing is null)
        {
            db.RenovationOverrides.Add(new ListingRenovationOverride(listingId, level, items, request.TotalBudget?.Amount, request.Notes, clock.GetUtcNow()));
        }
        else
        {
            if (http.Request.Headers.ContainsKey(HeaderNames.IfMatch) && ETags.Check(http.Request, existing.Version) is { } conflict)
            {
                return conflict;
            }

            existing.Update(level, items, request.TotalBudget?.Amount, request.Notes, clock.GetUtcNow());
        }

        await db.SaveChangesAsync(cancellationToken);
        var estimate = await RefreshAsync(listing, analysis, insights, db, cancellationToken);
        var stored = await db.RenovationOverrides.AsNoTracking().SingleAsync(candidate => candidate.ListingId == listingId, cancellationToken);
        ETags.Write(http.Response, stored.Version);
        return TypedResults.Ok(RenovationEstimateDto.From(estimate));
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteOverrideAsync(Guid listingId, IHomeRadarDbContext db, AnalysisService analysis, ListingInsightService insights, CancellationToken cancellationToken)
    {
        var existing = await db.RenovationOverrides.SingleOrDefaultAsync(candidate => candidate.ListingId == listingId, cancellationToken);
        if (existing is null)
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, title: "No renovation override", detail: $"Listing {listingId} has no manual renovation.", type: ProblemTypes.NotFound);
        }

        db.RenovationOverrides.Remove(existing);
        await db.SaveChangesAsync(cancellationToken);
        var listing = await db.Listings.SingleAsync(candidate => candidate.Id == listingId, cancellationToken);
        await RefreshAsync(listing, analysis, insights, db, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<CadastralDataDto>, ValidationProblem, ProblemHttpResult>> SetCadastralDataAsync(
        Guid listingId,
        CadastralDataRequestDto request,
        IHomeRadarDbContext db,
        AnalysisService analysis,
        ListingInsightService insights,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        CadastralReference? reference = null;
        if (request.CadastralReference is { } text)
        {
            if (CadastralReference.TryParse(text, out var parsed))
            {
                reference = parsed;
            }
            else
            {
                errors["cadastralReference"] = ["Must be a 20-character cadastral reference."];
            }
        }

        if (request.BuiltSquareMetres is <= 0m or > 100_000m)
        {
            errors["builtSquareMetres"] = ["Must be between 0 and 100000."];
        }

        if (request.YearBuilt is < 1500 or > 2100)
        {
            errors["yearBuilt"] = ["Must be between 1500 and 2100."];
        }

        if (request.ReferenceValue is { Amount: <= 0m })
        {
            errors["referenceValue"] = ["Must be positive."];
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        var listing = await db.Listings.SingleOrDefaultAsync(candidate => candidate.Id == listingId, cancellationToken);
        if (listing is null)
        {
            return Problems.From(AnalysisErrors.ListingNotFound(listingId));
        }

        if (reference is not null || request.BuiltSquareMetres is not null || request.YearBuilt is not null)
        {
            var current = listing.Cadastre;
            listing.SetCadastralFacts(new CadastralFacts(
                reference?.Value ?? current?.Reference,
                request.BuiltSquareMetres ?? current?.BuiltSquareMetres,
                request.YearBuilt ?? current?.YearBuilt,
                current?.Use,
                current?.IsRustic,
                CadastralMatchQuality.Manual));
        }

        listing.SetReferenceValue(request.ReferenceValue is { } value ? Money.Euros(value.Amount) : listing.ReferenceValue, request.ReferenceValueDate ?? listing.ReferenceValueDate);
        await RefreshAsync(listing, analysis, insights, db, cancellationToken);
        return TypedResults.Ok(new CadastralDataDto(listing.Cadastre is { } facts ? CadastreDto.From(facts) : null, MoneyDto.From(listing.ReferenceValue), listing.ReferenceValueDate));
    }

    private static async Task<Results<Ok<RenovationEstimateDto>, ValidationProblem, ProblemHttpResult>> EstimateAsync(RenovationEstimateRequestDto request, IHomeRadarDbContext db, AnalysisService analysis, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        RenovationLevel? level = null;
        if (request.Level is { } levelText)
        {
            if (Enum.TryParse<RenovationLevel>(levelText, ignoreCase: true, out var parsedLevel))
            {
                level = parsedLevel;
            }
            else
            {
                errors["level"] = ["Must be ReadyToMoveIn, Light, Medium or Full."];
            }
        }

        PropertyType? propertyType = null;
        if (request.PropertyType is { } typeText)
        {
            if (Enum.TryParse<PropertyType>(typeText, ignoreCase: true, out var parsedType))
            {
                propertyType = parsedType;
            }
            else
            {
                errors["propertyType"] = ["Unknown property type."];
            }
        }

        if (request.ListingId is null && request.BuiltSquareMetres is null && request.Description is null && request.YearBuilt is null && level is null)
        {
            errors["request"] = ["Give a listingId or at least one of builtSquareMetres, yearBuilt, description or level."];
        }

        if (request.BuiltSquareMetres is <= 0m or > 100_000m)
        {
            errors["builtSquareMetres"] = ["Must be between 0 and 100000."];
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        var configuration = await analysis.ConfigurationAsync(null, cancellationToken);
        if (!configuration.IsSuccess)
        {
            return Problems.From(configuration.Error!);
        }

        Listing? listing = null;
        RenovationOverride? manual = null;
        if (request.ListingId is { } listingId)
        {
            listing = await db.Listings.AsNoTracking().SingleOrDefaultAsync(candidate => candidate.Id == listingId, cancellationToken);
            if (listing is null)
            {
                return Problems.From(AnalysisErrors.ListingNotFound(listingId));
            }

            manual = (await db.RenovationOverrides.AsNoTracking().SingleOrDefaultAsync(candidate => candidate.ListingId == listingId, cancellationToken))?.ToOverride();
        }

        if (level is { } forced)
        {
            manual = new RenovationOverride(forced, [], null, null, DateTimeOffset.MinValue);
        }

        var input = new RenovationInput(
            listing?.Title,
            request.Description ?? listing?.Description,
            request.BuiltSquareMetres ?? listing?.BuiltSquareMetres,
            request.Bathrooms ?? listing?.Bathrooms,
            request.Rooms ?? listing?.Rooms,
            request.YearBuilt ?? listing?.EffectiveYearBuilt,
            request.IsNewBuild ?? listing?.IsNewBuild ?? false,
            propertyType ?? listing?.PropertyType ?? PropertyType.Flat,
            manual,
            analysis.Today.Year);
        var estimate = new RuleBasedRenovationEstimator().Estimate(input, configuration.Value!.Config.Renovation);
        return TypedResults.Ok(RenovationEstimateDto.From(estimate));
    }

    private static async Task<RenovationEstimate> RefreshAsync(Listing listing, AnalysisService analysis, ListingInsightService insights, IHomeRadarDbContext db, CancellationToken cancellationToken)
    {
        var profile = await analysis.ResolveProfileAsync(null, cancellationToken);
        var refreshed = await insights.RefreshAsync(listing, profile.IsSuccess ? profile.Value : null, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        if (refreshed.IsSuccess)
        {
            return refreshed.Value!.Renovation;
        }

        var configuration = await analysis.ConfigurationAsync(null, cancellationToken);
        return await analysis.RenovationAsync(listing, configuration.Value!.Config, cancellationToken);
    }
}
