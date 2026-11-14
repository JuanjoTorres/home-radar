using HomeRadar.Api.Contracts;
using HomeRadar.Api.Infrastructure;
using HomeRadar.Application.Ports;
using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace HomeRadar.Api.Endpoints;

public static class ProfileEndpoints
{
    public static RouteGroupBuilder MapProfileEndpoints(this RouteGroupBuilder group)
    {
        var financing = group.MapGroup("/financing-profiles");
        financing.MapGet("/", ListFinancingAsync).Document("listFinancingProfiles", "List financing profiles.", "Financing profiles").Produces<IReadOnlyList<FinancingProfileDto>>();
        financing.MapPost("/", CreateFinancingAsync).Document("createFinancingProfile", "Create a financing profile.", "Financing profiles").AddEndpointFilter<IdempotencyFilter>()
            .Produces<FinancingProfileDto>(StatusCodes.Status201Created).ProducesValidationProblem().ProducesProblem(StatusCodes.Status409Conflict);
        financing.MapGet("/{financingProfileId:guid}", GetFinancingAsync).Document("getFinancingProfile", "Get a financing profile.", "Financing profiles").Produces<FinancingProfileDto>().ProducesProblem(StatusCodes.Status404NotFound);
        financing.MapPut("/{financingProfileId:guid}", UpdateFinancingAsync).Document("updateFinancingProfile", "Replace a financing profile (requires If-Match).", "Financing profiles")
            .Produces<FinancingProfileDto>().ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status412PreconditionFailed).ProducesProblem(StatusCodes.Status428PreconditionRequired);
        financing.MapDelete("/{financingProfileId:guid}", DeleteFinancingAsync).Document("deleteFinancingProfile", "Delete a financing profile.", "Financing profiles").Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status404NotFound);

        var searches = group.MapGroup("/search-profiles");
        searches.MapGet("/", ListSearchAsync).Document("listSearchProfiles", "List saved searches.", "Search profiles").Produces<IReadOnlyList<SearchProfileDto>>();
        searches.MapPost("/", CreateSearchAsync).Document("createSearchProfile", "Create a saved search with alert channels.", "Search profiles").AddEndpointFilter<IdempotencyFilter>()
            .Produces<SearchProfileDto>(StatusCodes.Status201Created).ProducesValidationProblem().ProducesProblem(StatusCodes.Status409Conflict);
        searches.MapGet("/{searchProfileId:guid}", GetSearchAsync).Document("getSearchProfile", "Get a saved search.", "Search profiles").Produces<SearchProfileDto>().ProducesProblem(StatusCodes.Status404NotFound);
        searches.MapPut("/{searchProfileId:guid}", UpdateSearchAsync).Document("updateSearchProfile", "Replace a saved search (requires If-Match).", "Search profiles")
            .Produces<SearchProfileDto>().ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status412PreconditionFailed).ProducesProblem(StatusCodes.Status428PreconditionRequired);
        searches.MapDelete("/{searchProfileId:guid}", DeleteSearchAsync).Document("deleteSearchProfile", "Delete a saved search.", "Search profiles").Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<Ok<IReadOnlyList<FinancingProfileDto>>> ListFinancingAsync(IHomeRadarDbContext db, CancellationToken cancellationToken) =>
        TypedResults.Ok<IReadOnlyList<FinancingProfileDto>>((await db.FinancingProfiles.AsNoTracking().OrderBy(profile => profile.Key).ToListAsync(cancellationToken)).Select(FinancingProfileDto.From).ToList());

    private static async Task<Results<Created<FinancingProfileDto>, ValidationProblem, ProblemHttpResult>> CreateFinancingAsync(FinancingProfileInputDto input, IHomeRadarDbContext db, TimeProvider clock, HttpResponse response, CancellationToken cancellationToken)
    {
        var errors = ValidateFinancing(input);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        if (await db.FinancingProfiles.AnyAsync(profile => profile.Key == input.Key, cancellationToken))
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status409Conflict, title: "A financing profile with this key already exists", type: ProblemTypes.Conflict);
        }

        var profile = new FinancingProfile(input.Key, 0, input.Name, Money.Euros(input.Savings.Amount), Money.Euros(input.NetMonthlyIncome.Amount), Money.Euros(input.ExistingMonthlyDebt.Amount), input.MaxDebtToIncomeRatio, input.Mortgage.ToDomain(), input.Buyer.ToDomain(), input.IsDefault);
        profile.Update(0, input.Name, profile.Savings, profile.NetMonthlyIncome, profile.ExistingMonthlyDebt, profile.MaxDebtToIncomeRatio, profile.Mortgage, profile.Buyer, profile.IsDefault, clock.GetUtcNow());
        db.FinancingProfiles.Add(profile);
        await db.SaveChangesAsync(cancellationToken);
        ETags.Write(response, profile.Version);
        return TypedResults.Created($"/v1/financing-profiles/{profile.Id}", FinancingProfileDto.From(profile));
    }

    private static async Task<Results<Ok<FinancingProfileDto>, ProblemHttpResult>> GetFinancingAsync(Guid financingProfileId, IHomeRadarDbContext db, HttpResponse response, CancellationToken cancellationToken)
    {
        var profile = await db.FinancingProfiles.AsNoTracking().SingleOrDefaultAsync(candidate => candidate.Id == financingProfileId, cancellationToken);
        if (profile is null)
        {
            return NotFound("Financing profile", financingProfileId);
        }

        ETags.Write(response, profile.Version);
        return TypedResults.Ok(FinancingProfileDto.From(profile));
    }

    private static async Task<Results<Ok<FinancingProfileDto>, ValidationProblem, ProblemHttpResult>> UpdateFinancingAsync(Guid financingProfileId, FinancingProfileInputDto input, IHomeRadarDbContext db, TimeProvider clock, HttpRequest request, HttpResponse response, CancellationToken cancellationToken)
    {
        var errors = ValidateFinancing(input);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        var profile = await db.FinancingProfiles.SingleOrDefaultAsync(candidate => candidate.Id == financingProfileId, cancellationToken);
        if (profile is null)
        {
            return NotFound("Financing profile", financingProfileId);
        }

        if (ETags.Check(request, profile.Version) is ProblemHttpResult precondition)
        {
            return precondition;
        }

        profile.Update(profile.Revision, input.Name, Money.Euros(input.Savings.Amount), Money.Euros(input.NetMonthlyIncome.Amount), Money.Euros(input.ExistingMonthlyDebt.Amount), input.MaxDebtToIncomeRatio, input.Mortgage.ToDomain(), input.Buyer.ToDomain(), input.IsDefault, clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        ETags.Write(response, profile.Version);
        return TypedResults.Ok(FinancingProfileDto.From(profile));
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteFinancingAsync(Guid financingProfileId, IHomeRadarDbContext db, CancellationToken cancellationToken)
    {
        var profile = await db.FinancingProfiles.SingleOrDefaultAsync(candidate => candidate.Id == financingProfileId, cancellationToken);
        if (profile is null)
        {
            return NotFound("Financing profile", financingProfileId);
        }

        db.FinancingProfiles.Remove(profile);
        await db.SaveChangesAsync(cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<IReadOnlyList<SearchProfileDto>>> ListSearchAsync(IHomeRadarDbContext db, CancellationToken cancellationToken) =>
        TypedResults.Ok<IReadOnlyList<SearchProfileDto>>((await db.SearchProfiles.AsNoTracking().OrderBy(profile => profile.Key).ToListAsync(cancellationToken)).Select(ToDto).ToList());

    private static async Task<Results<Created<SearchProfileDto>, ValidationProblem, ProblemHttpResult>> CreateSearchAsync(SearchProfileInputDto input, IHomeRadarDbContext db, TimeProvider clock, HttpResponse response, CancellationToken cancellationToken)
    {
        var errors = ValidateSearch(input);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        if (await db.SearchProfiles.AnyAsync(profile => profile.Key == input.Key, cancellationToken))
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status409Conflict, title: "A search profile with this key already exists", type: ProblemTypes.Conflict);
        }

        var profile = new SearchProfile(input.Key, 0, input.Name, SearchMapping.ToFilter(input.Filter), input.Channels.Select(channel => Enum.Parse<AlertChannel>(channel, ignoreCase: true)).ToList(), input.Priority, input.Enabled);
        profile.Update(0, profile.Name, profile.Filter, profile.Channels, profile.Priority, profile.Enabled, clock.GetUtcNow());
        db.SearchProfiles.Add(profile);
        await db.SaveChangesAsync(cancellationToken);
        ETags.Write(response, profile.Version);
        return TypedResults.Created($"/v1/search-profiles/{profile.Id}", ToDto(profile));
    }

    private static async Task<Results<Ok<SearchProfileDto>, ProblemHttpResult>> GetSearchAsync(Guid searchProfileId, IHomeRadarDbContext db, HttpResponse response, CancellationToken cancellationToken)
    {
        var profile = await db.SearchProfiles.AsNoTracking().SingleOrDefaultAsync(candidate => candidate.Id == searchProfileId, cancellationToken);
        if (profile is null)
        {
            return NotFound("Search profile", searchProfileId);
        }

        ETags.Write(response, profile.Version);
        return TypedResults.Ok(ToDto(profile));
    }

    private static async Task<Results<Ok<SearchProfileDto>, ValidationProblem, ProblemHttpResult>> UpdateSearchAsync(Guid searchProfileId, SearchProfileInputDto input, IHomeRadarDbContext db, TimeProvider clock, HttpRequest request, HttpResponse response, CancellationToken cancellationToken)
    {
        var errors = ValidateSearch(input);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        var profile = await db.SearchProfiles.SingleOrDefaultAsync(candidate => candidate.Id == searchProfileId, cancellationToken);
        if (profile is null)
        {
            return NotFound("Search profile", searchProfileId);
        }

        if (ETags.Check(request, profile.Version) is ProblemHttpResult precondition)
        {
            return precondition;
        }

        profile.Update(profile.Revision, input.Name, SearchMapping.ToFilter(input.Filter), input.Channels.Select(channel => Enum.Parse<AlertChannel>(channel, ignoreCase: true)).ToList(), input.Priority, input.Enabled, clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        ETags.Write(response, profile.Version);
        return TypedResults.Ok(ToDto(profile));
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteSearchAsync(Guid searchProfileId, IHomeRadarDbContext db, CancellationToken cancellationToken)
    {
        var profile = await db.SearchProfiles.SingleOrDefaultAsync(candidate => candidate.Id == searchProfileId, cancellationToken);
        if (profile is null)
        {
            return NotFound("Search profile", searchProfileId);
        }

        db.SearchProfiles.Remove(profile);
        await db.SaveChangesAsync(cancellationToken);
        return TypedResults.NoContent();
    }

    private static SearchProfileDto ToDto(SearchProfile profile) => new()
    {
        Id = profile.Id,
        Key = profile.Key,
        Revision = profile.Revision,
        Name = profile.Name,
        Filter = SearchMapping.ToRequest(profile.Filter),
        Channels = profile.Channels.Select(channel => channel.ToString()).ToList(),
        Priority = profile.Priority,
        Enabled = profile.Enabled,
        UpdatedAt = profile.UpdatedAt,
    };

    private static ProblemHttpResult NotFound(string resource, Guid id) =>
        TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, title: $"{resource} not found", detail: $"No {resource.ToLowerInvariant()} with id {id}.", type: ProblemTypes.NotFound);

    private static Dictionary<string, string[]> ValidateFinancing(FinancingProfileInputDto input)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(input.Key) || input.Key.Length > 64)
        {
            errors["key"] = ["Required, at most 64 characters."];
        }

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            errors["name"] = ["Required."];
        }

        if (input.Savings.Amount < 0 || input.NetMonthlyIncome.Amount <= 0 || input.ExistingMonthlyDebt.Amount < 0)
        {
            errors["amounts"] = ["Savings and debt must be ≥ 0; income must be > 0."];
        }

        if (input.MaxDebtToIncomeRatio is < 0.1m or > 0.6m)
        {
            errors["maxDebtToIncomeRatio"] = ["Must be between 0.1 and 0.6."];
        }

        if (!Enum.TryParse<MortgageType>(input.Mortgage.Type, ignoreCase: true, out _))
        {
            errors["mortgage.type"] = ["Must be Fixed, Variable or Mixed."];
        }

        if (input.Mortgage.MaxLoanToValue is < 0.5m or > 1.0m || input.Mortgage.TermMonths is < 60 or > 480)
        {
            errors["mortgage"] = ["maxLoanToValue must be 0.5–1.0 and termMonths 60–480."];
        }

        if (input.Buyer.Age is < 18 or > 99)
        {
            errors["buyer.age"] = ["Must be between 18 and 99."];
        }

        return errors;
    }

    private static Dictionary<string, string[]> ValidateSearch(SearchProfileInputDto input)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(input.Key) || input.Key.Length > 64)
        {
            errors["key"] = ["Required, at most 64 characters."];
        }

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            errors["name"] = ["Required."];
        }

        var badChannels = input.Channels.Where(channel => !Enum.TryParse<AlertChannel>(channel, ignoreCase: true, out _)).ToList();
        if (badChannels.Count > 0)
        {
            errors["channels"] = [$"Unknown channels: {string.Join(", ", badChannels)}."];
        }

        try
        {
            SearchMapping.ToFilter(input.Filter);
        }
        catch (ArgumentException exception)
        {
            errors["filter"] = [exception.Message];
        }

        return errors;
    }
}
