using HomeRadar.Domain.Sources;

namespace HomeRadar.Api.Contracts;

/// <summary>A registered source and its health.</summary>
public sealed record SourceDto
{
    /// <summary>Identifier.</summary>
    public required Guid Id { get; init; }

    /// <summary>Stable key, e.g. solvia.</summary>
    public required string Key { get; init; }

    /// <summary>Display name.</summary>
    public required string Name { get; init; }

    /// <summary>Servicer, Bank, Agency, Feed, EmailAlert, Auction or Aggregator.</summary>
    public required string Kind { get; init; }

    /// <summary>Adapter key resolving the implementation.</summary>
    public required string AdapterKey { get; init; }

    /// <summary>Full-scan interval (ISO 8601 duration), e.g. PT6H.</summary>
    public required string PollInterval { get; init; }

    /// <summary>Quick-scan interval for priority sources.</summary>
    public string? PriorityPollInterval { get; init; }

    /// <summary>Priority flag.</summary>
    public required bool Priority { get; init; }

    /// <summary>Enabled flag.</summary>
    public required bool Enabled { get; init; }

    /// <summary>Healthy, Degraded, Failing or Disabled.</summary>
    public required string Status { get; init; }

    /// <summary>Last run that completed successfully (UTC).</summary>
    public DateTimeOffset? LastSuccessfulRunAt { get; init; }

    /// <summary>Last run of any outcome (UTC).</summary>
    public DateTimeOffset? LastRunAt { get; init; }

    /// <summary>Consecutive failed or degraded runs.</summary>
    public required int ConsecutiveFailures { get; init; }

    /// <summary>Listings seen in the last successful run.</summary>
    public required int LastListingCount { get; init; }

    public static SourceDto From(Source source) => new()
    {
        Id = source.Id,
        Key = source.Key,
        Name = source.Name,
        Kind = source.Kind.ToString(),
        AdapterKey = source.AdapterKey,
        PollInterval = System.Xml.XmlConvert.ToString(source.PollInterval),
        PriorityPollInterval = source.PriorityPollInterval is { } quick ? System.Xml.XmlConvert.ToString(quick) : null,
        Priority = source.Priority,
        Enabled = source.Enabled,
        Status = source.Status.ToString(),
        LastSuccessfulRunAt = source.LastSuccessfulRunAt,
        LastRunAt = source.LastRunAt,
        ConsecutiveFailures = source.ConsecutiveFailures,
        LastListingCount = source.LastListingCount,
    };
}

/// <summary>One execution of a source.</summary>
public sealed record SourceRunDto
{
    /// <summary>Start (UTC).</summary>
    public required DateTimeOffset StartedAt { get; init; }

    /// <summary>End (UTC); null while running.</summary>
    public DateTimeOffset? FinishedAt { get; init; }

    /// <summary>Whether it enumerated every listing.</summary>
    public required bool FullScan { get; init; }

    /// <summary>Running, Succeeded, Degraded or Failed.</summary>
    public required string Outcome { get; init; }

    /// <summary>Listings seen.</summary>
    public required int ListingsSeen { get; init; }

    /// <summary>New listings.</summary>
    public required int ListingsCreated { get; init; }

    /// <summary>Listings with changes.</summary>
    public required int ListingsChanged { get; init; }

    /// <summary>Listings marked withdrawn.</summary>
    public required int ListingsWithdrawn { get; init; }

    /// <summary>Errors while ingesting individual listings.</summary>
    public required int Errors { get; init; }

    /// <summary>Human-readable reason for degraded or failed runs.</summary>
    public string? Message { get; init; }

    public static SourceRunDto From(SourceRun run) => new()
    {
        StartedAt = run.StartedAt,
        FinishedAt = run.FinishedAt,
        FullScan = run.FullScan,
        Outcome = run.Outcome.ToString(),
        ListingsSeen = run.ListingsSeen,
        ListingsCreated = run.ListingsCreated,
        ListingsChanged = run.ListingsChanged,
        ListingsWithdrawn = run.ListingsWithdrawn,
        Errors = run.Errors,
        Message = run.Message,
    };
}

/// <summary>Source with its recent runs.</summary>
/// <param name="Source">The source.</param>
/// <param name="RecentRuns">Most recent runs, newest first.</param>
public sealed record SourceHealthDto(SourceDto Source, IReadOnlyList<SourceRunDto> RecentRuns);
