namespace HomeRadar.Domain.Sources;

public sealed class SourceRun
{
    private SourceRun()
    {
    }

    public SourceRun(Guid sourceId, DateTimeOffset startedAt, bool fullScan)
    {
        SourceId = sourceId;
        StartedAt = startedAt;
        FullScan = fullScan;
    }

    public Guid Id { get; private init; } = Guid.CreateVersion7();

    public Guid SourceId { get; private init; }

    public DateTimeOffset StartedAt { get; private init; }

    public DateTimeOffset? FinishedAt { get; private set; }

    public bool FullScan { get; private init; }

    public SourceRunOutcome Outcome { get; private set; } = SourceRunOutcome.Running;

    public int ListingsSeen { get; private set; }

    public int ListingsCreated { get; private set; }

    public int ListingsChanged { get; private set; }

    public int ListingsWithdrawn { get; private set; }

    public int Errors { get; private set; }

    public string? Message { get; private set; }

    public void CountSeen() => ListingsSeen++;

    public void CountCreated() => ListingsCreated++;

    public void CountChanged() => ListingsChanged++;

    public void CountWithdrawn() => ListingsWithdrawn++;

    public void CountError() => Errors++;

    public void Finish(SourceRunOutcome outcome, DateTimeOffset finishedAt, string? message = null)
    {
        Outcome = outcome;
        FinishedAt = finishedAt;
        Message = message;
    }
}

public enum SourceRunOutcome
{
    Running = 0,
    Succeeded = 1,
    Degraded = 2,
    Failed = 3,
}
