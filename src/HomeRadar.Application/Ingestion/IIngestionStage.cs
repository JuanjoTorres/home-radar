namespace HomeRadar.Application.Ingestion;

/// <summary>
/// One step of the ingestion pipeline. Stages run in <see cref="Order"/> for every fetched listing;
/// each one decides whether it has work by inspecting the <see cref="IngestionContext"/>.
/// </summary>
public interface IIngestionStage
{
    /// <summary>Position in the pipeline; lower runs first. Reserve gaps so new stages (LLM enrichment) can slot in.</summary>
    int Order { get; }

    /// <summary>Processes the listing in the context. Must not call SaveChanges; the orchestrator owns the transaction.</summary>
    /// <param name="context">Shared state for this listing within the current run.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ExecuteAsync(IngestionContext context, CancellationToken cancellationToken);
}

public static class StageOrder
{
    public const int DetectChanges = 100;
    public const int Enrich = 200;
    public const int Deduplicate = 300;
    public const int Analyze = 400;
    public const int Match = 500;
    public const int Notify = 600;
}
