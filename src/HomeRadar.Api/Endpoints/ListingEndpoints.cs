using System.Linq.Expressions;
using HomeRadar.Api.Contracts;
using HomeRadar.Api.Infrastructure;
using HomeRadar.Application.Analysis;
using HomeRadar.Application.Ports;
using HomeRadar.Application.Search;
using HomeRadar.Domain.Listings;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeRadar.Api.Endpoints;

public static class ListingEndpoints
{
    private const string Tag = "Listings";

    public static RouteGroupBuilder MapListingEndpoints(this RouteGroupBuilder group)
    {
        var listings = group.MapGroup("/listings");

        listings.MapGet("/", ListAsync).Document("listListings", "List listings with simple query-string filters.", Tag)
            .Produces<PageDto<ListingSummaryDto>>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        listings.MapPost("/search", SearchAsync).Document("searchListings", "Search listings with full filters.", Tag)
            .Produces<PageDto<ListingSummaryDto>>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        listings.MapGet("/{listingId:guid}", GetAsync).Document("getListing", "Get a listing with its photos, features and sibling listings.", Tag)
            .Produces<ListingDetailDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        listings.MapGet("/{listingId:guid}/history", HistoryAsync).Document("getListingHistory", "Timeline of every detected change.", Tag)
            .Produces<PageDto<ListingEventDto>>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<Results<Ok<PageDto<ListingSummaryDto>>, ValidationProblem, ProblemHttpResult>> ListAsync(
        [FromQuery] string? municipality,
        [FromQuery] decimal? priceMin,
        [FromQuery] decimal? priceMax,
        [FromQuery] string? sourceKey,
        [FromQuery] int? newWithinHours,
        [FromQuery] Guid? affordableFor,
        [FromQuery] decimal? budget,
        [FromQuery] ListingSortDto? sort,
        [FromQuery] string? cursor,
        [FromQuery] int? limit,
        IHomeRadarDbContext db,
        AffordabilityLimitResolver affordability,
        TimeProvider clock,
        CancellationToken cancellationToken)
    {
        var request = new ListingSearchRequestDto
        {
            Municipalities = municipality is null ? null : [municipality],
            Price = priceMin is null && priceMax is null ? null : new DecimalRangeDto(priceMin, priceMax),
            SourceKeys = sourceKey is null ? null : [sourceKey],
            NewWithinHours = newWithinHours,
            Affordability = affordableFor is null && budget is null ? null : new AffordabilityFilterDto { FinancingProfileId = affordableFor, Budget = budget is { } amount ? new MoneyDto(amount) : null },
            Sort = sort ?? ListingSortDto.FirstSeenDesc,
            Cursor = cursor,
            Limit = limit ?? 25,
        };
        return await SearchAsync(request, db, affordability, clock, cancellationToken);
    }

    private static async Task<Results<Ok<PageDto<ListingSummaryDto>>, ValidationProblem, ProblemHttpResult>> SearchAsync(ListingSearchRequestDto request, IHomeRadarDbContext db, AffordabilityLimitResolver affordability, TimeProvider clock, CancellationToken cancellationToken)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        var now = clock.GetUtcNow();
        var filter = SearchMapping.ToFilter(request);
        AffordablePriceLimits? limits = null;
        if (filter.Affordability is { } criteria)
        {
            var resolved = await affordability.ResolveAsync(criteria, cancellationToken);
            if (!resolved.IsSuccess)
            {
                return Problems.From(resolved.Error!);
            }

            limits = resolved.Value;
        }

        var query = db.Listings.AsNoTracking().Where(ListingFilterSpecification.Build(filter, now, limits));
        var cursorValue = Cursor.Decode(request.Cursor);
        query = ApplyOrderAndCursor(query, request.Sort, cursorValue);

        var rows = await query.Take(request.Limit + 1).ToListAsync(cancellationToken);
        var hasMore = rows.Count > request.Limit;
        var page = rows.Take(request.Limit).ToList();
        var next = hasMore && page.Count > 0 ? CursorFor(page[^1], request.Sort).Encode() : null;
        return TypedResults.Ok(new PageDto<ListingSummaryDto>(page.Select(listing => ListingSummaryDto.From(listing, now)).ToList(), next, hasMore));
    }

    private static async Task<Results<Ok<ListingDetailDto>, ProblemHttpResult>> GetAsync(Guid listingId, IHomeRadarDbContext db, TimeProvider clock, CancellationToken cancellationToken)
    {
        var listing = await db.Listings.AsNoTracking().SingleOrDefaultAsync(candidate => candidate.Id == listingId, cancellationToken);
        if (listing is null)
        {
            return NotFound(listingId);
        }

        var siblings = listing.DwellingId is { } dwellingId
            ? await db.Listings.AsNoTracking().Where(candidate => candidate.DwellingId == dwellingId && candidate.Id != listingId).ToListAsync(cancellationToken)
            : [];
        return TypedResults.Ok(ListingDetailDto.From(listing, siblings, clock.GetUtcNow()));
    }

    private static async Task<Results<Ok<PageDto<ListingEventDto>>, ProblemHttpResult>> HistoryAsync(Guid listingId, [FromQuery] string? cursor, [FromQuery] int? limit, IHomeRadarDbContext db, CancellationToken cancellationToken)
    {
        if (!await db.Listings.AnyAsync(candidate => candidate.Id == listingId, cancellationToken))
        {
            return NotFound(listingId);
        }

        var pageSize = limit is null or <= 0 or > 200 ? 50 : limit.Value;
        var query = db.ListingEvents.AsNoTracking().Where(listingEvent => listingEvent.ListingId == listingId);
        if (Cursor.Decode(cursor) is { } cursorValue && cursorValue.AsTimestamp() is { } before)
        {
            query = query.Where(listingEvent => listingEvent.OccurredAt < before || (listingEvent.OccurredAt == before && listingEvent.Id.CompareTo(cursorValue.Id) < 0));
        }

        var rows = await query.OrderByDescending(listingEvent => listingEvent.OccurredAt).ThenByDescending(listingEvent => listingEvent.Id).Take(pageSize + 1).ToListAsync(cancellationToken);
        var hasMore = rows.Count > pageSize;
        var page = rows.Take(pageSize).ToList();
        var next = hasMore ? new Cursor(Cursor.Format(page[^1].OccurredAt), page[^1].Id).Encode() : null;
        return TypedResults.Ok(new PageDto<ListingEventDto>(page.Select(ListingEventDto.From).ToList(), next, hasMore));
    }

    private static ProblemHttpResult NotFound(Guid listingId) =>
        TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, title: "Listing not found", detail: $"No listing with id {listingId}.", type: ProblemTypes.NotFound);

    private static Dictionary<string, string[]> Validate(ListingSearchRequestDto request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request.Limit is < 1 or > 100)
        {
            errors["limit"] = ["Must be between 1 and 100."];
        }

        if (request.NewWithinHours is < 1 or > 720)
        {
            errors["newWithinHours"] = ["Must be between 1 and 720."];
        }

        if (request.Affordability is { } affordability)
        {
            if (affordability.FinancingProfileId is null && affordability.Budget is null)
            {
                errors["affordability"] = ["Needs financingProfileId, budget or both."];
            }

            if (affordability.Budget is { Amount: <= 0 })
            {
                errors["affordability.budget"] = ["Must be positive."];
            }

            if (!Enum.TryParse<Domain.Finance.AffordabilityVerdict>(affordability.MinimumVerdict, ignoreCase: true, out var verdict) || verdict == Domain.Finance.AffordabilityVerdict.NotAffordable)
            {
                errors["affordability.minimumVerdict"] = ["Must be Affordable or Borderline."];
            }
        }

        if (request.Radius is { } radius && (radius.Metres < 50 || radius.Metres > 50_000))
        {
            errors["radius.metres"] = ["Must be between 50 and 50000."];
        }

        if (request.Cursor is { Length: > 0 } && Cursor.Decode(request.Cursor) is null)
        {
            errors["cursor"] = ["Cursor is not valid."];
        }

        TryParseAll<Domain.ValueObjects.PropertyType>(request.PropertyTypes, "propertyTypes", errors);
        TryParseAll<Domain.ValueObjects.SellerType>(request.SellerTypes, "sellerTypes", errors);
        TryParseAll<ListingStatus>(request.Statuses, "statuses", errors);
        TryParseAll<Domain.ValueObjects.PossessionStatus>(request.ExcludePossession, "excludePossession", errors);
        return errors;
    }

    private static void TryParseAll<T>(IReadOnlyList<string>? values, string field, Dictionary<string, string[]> errors)
        where T : struct, Enum
    {
        var invalid = values?.Where(value => !Enum.TryParse<T>(value, ignoreCase: true, out _)).ToList() ?? [];
        if (invalid.Count > 0)
        {
            errors[field] = [$"Unknown values: {string.Join(", ", invalid)}. Allowed: {string.Join(", ", Enum.GetNames<T>())}."];
        }
    }

    private static IQueryable<Listing> ApplyOrderAndCursor(IQueryable<Listing> query, ListingSortDto sort, Cursor? cursor)
    {
        switch (sort)
        {
            case ListingSortDto.PriceAsc:
                if (cursor?.AsDecimal() is { } priceAfter)
                {
                    query = query.Where(l => l.PriceAmount > priceAfter || (l.PriceAmount == priceAfter && l.Id.CompareTo(cursor.Id) > 0));
                }

                return query.OrderBy(l => l.PriceAmount).ThenBy(l => l.Id);
            case ListingSortDto.PriceDesc:
                if (cursor?.AsDecimal() is { } priceBefore)
                {
                    query = query.Where(l => l.PriceAmount < priceBefore || (l.PriceAmount == priceBefore && l.Id.CompareTo(cursor.Id) < 0));
                }

                return query.OrderByDescending(l => l.PriceAmount).ThenByDescending(l => l.Id);
            case ListingSortDto.PricePerM2:
                Expression<Func<Listing, decimal?>> ppm2 = l => l.PriceAmount / l.BuiltSquareMetres;
                if (cursor?.AsDecimal() is { } ppm2After)
                {
                    query = query.Where(l => l.PriceAmount / l.BuiltSquareMetres > ppm2After || (l.PriceAmount / l.BuiltSquareMetres == ppm2After && l.Id.CompareTo(cursor.Id) > 0));
                }

                return query.OrderBy(ppm2).ThenBy(l => l.Id);
            case ListingSortDto.PriceDropDesc:
                if (cursor?.AsDecimal() is { } dropBefore)
                {
                    query = query.Where(l => l.PriceDropCount < dropBefore || (l.PriceDropCount == dropBefore && l.Id.CompareTo(cursor.Id) < 0));
                }

                return query.OrderByDescending(l => l.PriceDropCount).ThenByDescending(l => l.Id);
            default:
                if (cursor?.AsTimestamp() is { } seenBefore)
                {
                    query = query.Where(l => l.FirstSeenAt < seenBefore || (l.FirstSeenAt == seenBefore && l.Id.CompareTo(cursor.Id) < 0));
                }

                return query.OrderByDescending(l => l.FirstSeenAt).ThenByDescending(l => l.Id);
        }
    }

    private static Cursor CursorFor(Listing last, ListingSortDto sort) => sort switch
    {
        ListingSortDto.PriceAsc or ListingSortDto.PriceDesc => new Cursor(Cursor.Format(last.PriceAmount ?? 0), last.Id),
        ListingSortDto.PricePerM2 => new Cursor(Cursor.Format(last.PricePerSquareMetre ?? 0), last.Id),
        ListingSortDto.PriceDropDesc => new Cursor(Cursor.Format(last.PriceDropCount), last.Id),
        _ => new Cursor(Cursor.Format(last.FirstSeenAt), last.Id),
    };
}
