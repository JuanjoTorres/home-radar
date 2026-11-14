using HomeRadar.Application.Seeding;
using HomeRadar.Infrastructure.Seeding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace HomeRadar.Infrastructure.Scheduling;

[DisallowConcurrentExecution]
public sealed partial class SeedJob(SeedService seeds, IOptions<SeedOptions> options, ILogger<SeedJob> logger) : IJob
{
    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            return;
        }

        var directory = Path.GetFullPath(options.Value.Directory);
        if (!Directory.Exists(directory))
        {
            MissingDirectory(logger, directory);
            return;
        }

        var sources = YamlSeedLoader.LoadSources(directory);
        var financing = YamlSeedLoader.LoadFinancingProfiles(directory);
        var searches = YamlSeedLoader.LoadSearchProfiles(directory);
        await seeds.ApplyAsync(sources, financing, searches, cancellationToken);
        Applied(logger, sources.Count, financing.Count, searches.Count);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Seed directory {Directory} does not exist; nothing seeded")]
    private static partial void MissingDirectory(ILogger logger, string directory);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seed applied: {Sources} sources, {Financing} financing profiles, {Searches} search profiles")]
    private static partial void Applied(ILogger logger, int sources, int financing, int searches);
}
