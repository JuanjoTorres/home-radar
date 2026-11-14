using HomeRadar.Application.Ingestion;
using Microsoft.Extensions.Logging;
using Quartz;

namespace HomeRadar.Infrastructure.Scheduling;

[DisallowConcurrentExecution]
public sealed partial class SourcePollJob(SourceIngestionService ingestion, ILogger<SourcePollJob> logger) : IJob
{
    public const string SourceIdKey = "sourceId";
    public const string FullScanKey = "fullScan";

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken)
    {
        var sourceId = Guid.Parse(context.MergedJobDataMap[SourceIdKey]!.ToString()!);
        var fullScan = string.Equals(context.MergedJobDataMap[FullScanKey]?.ToString(), "true", StringComparison.OrdinalIgnoreCase);
        var run = await ingestion.RunAsync(sourceId, fullScan, cancellationToken);
        Finished(logger, sourceId, fullScan, run.Outcome, run.ListingsSeen, run.ListingsCreated, run.ListingsChanged, run.ListingsWithdrawn, run.Errors);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Source {SourceId} polled (full={FullScan}): {Outcome}, seen={Seen} created={Created} changed={Changed} withdrawn={Withdrawn} errors={Errors}")]
    private static partial void Finished(ILogger logger, Guid sourceId, bool fullScan, Domain.Sources.SourceRunOutcome outcome, int seen, int created, int changed, int withdrawn, int errors);
}
