using HomeRadar.Api.Contracts;
using HomeRadar.Api.Infrastructure;
using HomeRadar.Application.Ports;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace HomeRadar.Api.Endpoints;

public static class SourceEndpoints
{
    private const string Tag = "Sources";

    public static RouteGroupBuilder MapSourceEndpoints(this RouteGroupBuilder group)
    {
        var sources = group.MapGroup("/sources");
        sources.MapGet("/", ListAsync).Document("listSources", "Registered sources and their health.", Tag).Produces<IReadOnlyList<SourceDto>>();
        sources.MapGet("/{sourceId:guid}/health", HealthAsync).Document("getSourceHealth", "Recent runs, counters and errors of a source.", Tag).Produces<SourceHealthDto>().ProducesProblem(StatusCodes.Status404NotFound);
        return group;
    }

    private static async Task<Ok<IReadOnlyList<SourceDto>>> ListAsync(IHomeRadarDbContext db, CancellationToken cancellationToken) =>
        TypedResults.Ok<IReadOnlyList<SourceDto>>((await db.Sources.AsNoTracking().OrderBy(source => source.Key).ToListAsync(cancellationToken)).Select(SourceDto.From).ToList());

    private static async Task<Results<Ok<SourceHealthDto>, ProblemHttpResult>> HealthAsync(Guid sourceId, IHomeRadarDbContext db, CancellationToken cancellationToken)
    {
        var source = await db.Sources.AsNoTracking().SingleOrDefaultAsync(candidate => candidate.Id == sourceId, cancellationToken);
        if (source is null)
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, title: "Source not found", type: ProblemTypes.NotFound);
        }

        var runs = await db.SourceRuns.AsNoTracking().Where(run => run.SourceId == sourceId).OrderByDescending(run => run.StartedAt).Take(20).ToListAsync(cancellationToken);
        return TypedResults.Ok(new SourceHealthDto(SourceDto.From(source), runs.Select(SourceRunDto.From).ToList()));
    }
}
