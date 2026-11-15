using HomeRadar.Application.Intelligence;
using Microsoft.Extensions.Logging;
using Quartz;

namespace HomeRadar.Infrastructure.Scheduling;

[DisallowConcurrentExecution]
public sealed partial class RescoreJob(IntelligenceRefreshService refresh, ILogger<RescoreJob> logger) : IJob
{
    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken)
    {
        var refreshed = await refresh.RefreshActiveAsync(cancellationToken);
        Rescored(logger, refreshed);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Rescored {Count} active listings")]
    private static partial void Rescored(ILogger logger, int count);
}
