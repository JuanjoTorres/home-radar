using HomeRadar.Domain.Common;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Domain.Sources;

public sealed class Source : Entity
{
    private Source()
    {
    }

    public Source(string key, string name, SourceKind kind, SellerType sellerType, string adapterKey, TimeSpan pollInterval, bool priority, string configurationJson)
    {
        Key = key;
        Name = name;
        Kind = kind;
        SellerType = sellerType;
        AdapterKey = adapterKey;
        PollInterval = pollInterval;
        Priority = priority;
        ConfigurationJson = configurationJson;
    }

    public string Key { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public SourceKind Kind { get; private set; }

    public SellerType SellerType { get; private set; }

    public string AdapterKey { get; private set; } = string.Empty;

    public TimeSpan PollInterval { get; private set; }

    public TimeSpan? PriorityPollInterval { get; private set; }

    public bool Priority { get; private set; }

    public bool Enabled { get; private set; } = true;

    public SourceStatus Status { get; private set; } = SourceStatus.Healthy;

    public DateTimeOffset? LastSuccessfulRunAt { get; private set; }

    public DateTimeOffset? LastRunAt { get; private set; }

    public int ConsecutiveFailures { get; private set; }

    public int LastListingCount { get; private set; }

    public string ConfigurationJson { get; private set; } = "{}";

    public void Configure(TimeSpan pollInterval, TimeSpan? priorityPollInterval, bool priority, bool enabled, string configurationJson)
    {
        PollInterval = pollInterval;
        PriorityPollInterval = priorityPollInterval;
        Priority = priority;
        Enabled = enabled;
        ConfigurationJson = configurationJson;
    }

    public void RecordRun(SourceRun run)
    {
        LastRunAt = run.FinishedAt ?? run.StartedAt;
        switch (run.Outcome)
        {
            case SourceRunOutcome.Succeeded:
                ConsecutiveFailures = 0;
                LastSuccessfulRunAt = run.FinishedAt;
                LastListingCount = run.ListingsSeen;
                Status = SourceStatus.Healthy;
                break;
            case SourceRunOutcome.Degraded:
                ConsecutiveFailures++;
                Status = SourceStatus.Degraded;
                break;
            case SourceRunOutcome.Failed:
                ConsecutiveFailures++;
                Status = ConsecutiveFailures >= 3 ? SourceStatus.Failing : SourceStatus.Degraded;
                break;
            default:
                throw new DomainException($"Unknown run outcome {run.Outcome}.");
        }
    }

    public void Disable() => Enabled = false;
}

public enum SourceKind
{
    Servicer = 0,
    Bank = 1,
    Agency = 2,
    Feed = 3,
    EmailAlert = 4,
    Auction = 5,
    Aggregator = 6,
}

public enum SourceStatus
{
    Healthy = 0,
    Degraded = 1,
    Failing = 2,
    Disabled = 3,
}
