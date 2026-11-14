using HomeRadar.Application.Outbox;
using Quartz;

namespace HomeRadar.Infrastructure.Scheduling;

[DisallowConcurrentExecution]
public sealed class OutboxDispatchJob(OutboxDispatcher dispatcher) : IJob
{
    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken)
    {
        int dispatched;
        do
        {
            dispatched = await dispatcher.DispatchPendingAsync(cancellationToken);
        }
        while (dispatched > 0 && !cancellationToken.IsCancellationRequested);
    }
}
