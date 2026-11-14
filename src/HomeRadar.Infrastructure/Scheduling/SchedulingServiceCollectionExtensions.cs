using HomeRadar.Infrastructure.Seeding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace HomeRadar.Infrastructure.Scheduling;

public static class SchedulingServiceCollectionExtensions
{
    public static IServiceCollection AddHomeRadarScheduling(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SeedOptions>().Bind(configuration.GetSection(SeedOptions.SectionName));
        services.AddQuartz(quartz =>
        {
            quartz.ConfigureScheduler(options => options.InstanceName = "homeradar");
            quartz.UseDefaultThreadPool(pool => pool.MaxConcurrency = 4);

            var seedKey = new JobKey("seed", "system");
            quartz.AddJob<SeedJob>(job => job.WithIdentity(seedKey).StoreDurably());
            quartz.AddTrigger<SeedJob>(trigger => trigger.ForJob(seedKey).WithIdentity("seed:startup", "system").StartNow());

            var syncKey = new JobKey("schedule-sync", "system");
            quartz.AddJob<ScheduleSyncJob>(job => job.WithIdentity(syncKey).StoreDurably());
            quartz.AddTrigger<ScheduleSyncJob>(trigger => trigger.ForJob(syncKey).WithIdentity("schedule-sync:periodic", "system")
                .StartAt(DateTimeOffset.UtcNow.AddSeconds(15))
                .WithSchedule(SimpleScheduleBuilder.Create().WithInterval(TimeSpan.FromMinutes(5)).RepeatForever()));

            var outboxKey = new JobKey("outbox", "system");
            quartz.AddJob<OutboxDispatchJob>(job => job.WithIdentity(outboxKey).StoreDurably());
            quartz.AddTrigger<OutboxDispatchJob>(trigger => trigger.ForJob(outboxKey).WithIdentity("outbox:periodic", "system")
                .StartAt(DateTimeOffset.UtcNow.AddSeconds(20))
                .WithSchedule(SimpleScheduleBuilder.Create().WithInterval(TimeSpan.FromSeconds(30)).RepeatForever()));
        });
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
        return services;
    }
}
