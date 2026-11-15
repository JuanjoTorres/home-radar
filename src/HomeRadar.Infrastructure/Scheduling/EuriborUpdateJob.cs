using HomeRadar.Application.ReferenceData;
using Microsoft.Extensions.Logging;
using Quartz;

namespace HomeRadar.Infrastructure.Scheduling;

[DisallowConcurrentExecution]
public sealed partial class EuriborUpdateJob(EuriborUpdateService euribor, ILogger<EuriborUpdateJob> logger) : IJob
{
    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken)
    {
        try
        {
            if (await euribor.UpdateAsync(cancellationToken) is { } rate)
            {
                Stored(logger, rate.MonthLabel, rate.Percent);
            }
        }
        catch (HttpRequestException exception)
        {
            Failed(logger, exception.Message);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Euríbor 12M for {Month} stored: {Percent} %")]
    private static partial void Stored(ILogger logger, string month, decimal percent);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Euríbor update failed: {Reason}")]
    private static partial void Failed(ILogger logger, string reason);
}
