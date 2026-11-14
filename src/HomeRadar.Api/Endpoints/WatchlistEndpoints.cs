using HomeRadar.Api.Contracts;
using HomeRadar.Api.Infrastructure;
using HomeRadar.Application.Ports;
using HomeRadar.Domain.Watchlist;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace HomeRadar.Api.Endpoints;

public static class WatchlistEndpoints
{
    private const string Tag = "Watchlist";

    public static RouteGroupBuilder MapWatchlistEndpoints(this RouteGroupBuilder group)
    {
        var watchlist = group.MapGroup("/watchlist");
        watchlist.MapGet("/", ListAsync).Document("listWatchlist", "List followed listings.", Tag).Produces<IReadOnlyList<WatchlistItemDto>>();
        watchlist.MapPost("/", AddAsync).Document("addToWatchlist", "Follow a listing; every later change is notified with a diff.", Tag).AddEndpointFilter<IdempotencyFilter>()
            .Produces<WatchlistItemDto>(StatusCodes.Status201Created).ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status409Conflict);
        watchlist.MapPatch("/{watchlistItemId:guid}", PatchAsync).Document("updateWatchlistItem", "Update status or notes (requires If-Match).", Tag)
            .Produces<WatchlistItemDto>().ProducesValidationProblem().ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status412PreconditionFailed).ProducesProblem(StatusCodes.Status428PreconditionRequired);
        watchlist.MapDelete("/{watchlistItemId:guid}", RemoveAsync).Document("removeFromWatchlist", "Stop following a listing.", Tag).Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status404NotFound);
        return group;
    }

    private static async Task<Ok<IReadOnlyList<WatchlistItemDto>>> ListAsync(IHomeRadarDbContext db, TimeProvider clock, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var rows = await db.WatchlistItems.AsNoTracking()
            .Join(db.Listings.AsNoTracking(), item => item.ListingId, listing => listing.Id, (item, listing) => new { item, listing })
            .OrderByDescending(pair => pair.item.AddedAt)
            .ToListAsync(cancellationToken);
        return TypedResults.Ok<IReadOnlyList<WatchlistItemDto>>(rows.Select(pair => ToDto(pair.item, pair.listing, now)).ToList());
    }

    private static async Task<Results<Created<WatchlistItemDto>, ProblemHttpResult>> AddAsync(WatchlistItemInputDto input, IHomeRadarDbContext db, TimeProvider clock, HttpResponse response, CancellationToken cancellationToken)
    {
        var listing = await db.Listings.AsNoTracking().SingleOrDefaultAsync(candidate => candidate.Id == input.ListingId, cancellationToken);
        if (listing is null)
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, title: "Listing not found", type: ProblemTypes.NotFound);
        }

        if (await db.WatchlistItems.AnyAsync(item => item.ListingId == input.ListingId, cancellationToken))
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status409Conflict, title: "Listing is already on the watchlist", type: ProblemTypes.Conflict);
        }

        var now = clock.GetUtcNow();
        var item = new WatchlistItem(input.ListingId, input.Notes, now);
        db.WatchlistItems.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        ETags.Write(response, item.Version);
        return TypedResults.Created($"/v1/watchlist/{item.Id}", ToDto(item, listing, now));
    }

    private static async Task<Results<Ok<WatchlistItemDto>, ValidationProblem, ProblemHttpResult>> PatchAsync(Guid watchlistItemId, WatchlistItemPatchDto patch, IHomeRadarDbContext db, TimeProvider clock, HttpRequest request, HttpResponse response, CancellationToken cancellationToken)
    {
        WatchlistStatus? status = null;
        if (patch.Status is { Length: > 0 })
        {
            if (!Enum.TryParse<WatchlistStatus>(patch.Status, ignoreCase: true, out var parsed))
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["status"] = [$"Allowed: {string.Join(", ", Enum.GetNames<WatchlistStatus>())}."] }, type: ProblemTypes.Validation);
            }

            status = parsed;
        }

        var item = await db.WatchlistItems.SingleOrDefaultAsync(candidate => candidate.Id == watchlistItemId, cancellationToken);
        if (item is null)
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, title: "Watchlist item not found", type: ProblemTypes.NotFound);
        }

        if (ETags.Check(request, item.Version) is ProblemHttpResult precondition)
        {
            return precondition;
        }

        var now = clock.GetUtcNow();
        item.Update(status, patch.Notes, now);
        await db.SaveChangesAsync(cancellationToken);
        var listing = await db.Listings.AsNoTracking().SingleAsync(candidate => candidate.Id == item.ListingId, cancellationToken);
        ETags.Write(response, item.Version);
        return TypedResults.Ok(ToDto(item, listing, now));
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> RemoveAsync(Guid watchlistItemId, IHomeRadarDbContext db, CancellationToken cancellationToken)
    {
        var item = await db.WatchlistItems.SingleOrDefaultAsync(candidate => candidate.Id == watchlistItemId, cancellationToken);
        if (item is null)
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, title: "Watchlist item not found", type: ProblemTypes.NotFound);
        }

        db.WatchlistItems.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return TypedResults.NoContent();
    }

    private static WatchlistItemDto ToDto(WatchlistItem item, Domain.Listings.Listing listing, DateTimeOffset now) => new()
    {
        Id = item.Id,
        Listing = ListingSummaryDto.From(listing, now),
        Status = item.Status.ToString(),
        Notes = item.Notes,
        AddedAt = item.AddedAt,
        UpdatedAt = item.UpdatedAt,
    };
}
