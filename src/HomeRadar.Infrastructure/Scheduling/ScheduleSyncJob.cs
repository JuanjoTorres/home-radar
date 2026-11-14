using HomeRadar.Application.Ports;
using HomeRadar.Domain.Sources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace HomeRadar.Infrastructure.Scheduling;

/// <summary>Keeps one full-scan trigger (and one quick-scan trigger for priority sources) per enabled source, mirroring the Sources table.</summary>
[DisallowConcurrentExecution]
public sealed partial class ScheduleSyncJob(IHomeRadarDbContext db, TimeProvider clock, ILogger<ScheduleSyncJob> logger) : IJob
{
    public const string GroupName = "sources";

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken)
    {
        var scheduler = context.Scheduler;
        var sources = await db.Sources.AsNoTracking().ToListAsync(cancellationToken);
        var wanted = new HashSet<JobKey>();

        foreach (var source in sources.Where(source => source.Enabled))
        {
            wanted.Add(await EnsureAsync(scheduler, source, fullScan: true, source.PollInterval, clock, cancellationToken));
            if (source.Priority && source.PriorityPollInterval is { } quick && quick < source.PollInterval)
            {
                wanted.Add(await EnsureAsync(scheduler, source, fullScan: false, quick, clock, cancellationToken));
            }
        }

        foreach (var key in await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(GroupName), cancellationToken))
        {
            if (!wanted.Contains(key))
            {
                await scheduler.DeleteJob(key, cancellationToken);
                Removed(logger, key.Name);
            }
        }
    }

    private static async Task<JobKey> EnsureAsync(IScheduler scheduler, Source source, bool fullScan, TimeSpan interval, TimeProvider clock, CancellationToken cancellationToken)
    {
        var jobKey = new JobKey($"{source.Key}:{(fullScan ? "full" : "quick")}", GroupName);
        var triggerKey = new TriggerKey(jobKey.Name, GroupName);
        var existing = await scheduler.GetTrigger(triggerKey, cancellationToken);
        if (existing is ISimpleTrigger simple && simple.RepeatInterval == interval)
        {
            return jobKey;
        }

        var job = JobBuilder.Create<SourcePollJob>()
            .WithIdentity(jobKey)
            .UsingJobData(SourcePollJob.SourceIdKey, source.Id.ToString())
            .UsingJobData(SourcePollJob.FullScanKey, fullScan ? "true" : "false")
            .StoreDurably()
            .Build();
        var now = clock.GetUtcNow();
        var startAt = source.LastRunAt is { } last && last + interval > now ? last + interval : now.AddSeconds(Random.Shared.Next(5, 60));
        var trigger = TriggerBuilder.Create<SourcePollJob>(clock)
            .WithIdentity(triggerKey)
            .ForJob(jobKey)
            .StartAt(startAt)
            .WithSchedule(SimpleScheduleBuilder.Create().WithInterval(interval).RepeatForever().WithMisfireInstruction(SimpleTriggerMisfireInstruction.NextWithRemainingCount))
            .Build();
        await scheduler.ScheduleJob(job, [trigger], new ScheduleJobOptions { Replace = true }, cancellationToken);
        return jobKey;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Removed schedule {JobName} (source disabled or deleted)")]
    private static partial void Removed(ILogger logger, string jobName);
}
