namespace HomeRadar.Domain.Outbox;

public sealed class OutboxMessage
{
    public const int MaxAttempts = 8;

    private OutboxMessage()
    {
    }

    public OutboxMessage(string type, string payload, DateTimeOffset createdAt)
    {
        Type = type;
        Payload = payload;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private init; } = Guid.CreateVersion7();

    public string Type { get; private init; } = string.Empty;

    public string Payload { get; private init; } = "{}";

    public DateTimeOffset CreatedAt { get; private init; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public DateTimeOffset? NextAttemptAt { get; private set; }

    public int Attempts { get; private set; }

    public string? LastError { get; private set; }

    public void MarkProcessed(DateTimeOffset now) => ProcessedAt = now;

    public void MarkFailed(DateTimeOffset now, string error)
    {
        Attempts++;
        LastError = error.Length > 2000 ? error[..2000] : error;
        var backoffMinutes = Math.Min(Math.Pow(2, Attempts), 120);
        NextAttemptAt = now.AddMinutes(backoffMinutes);
    }

    public bool Exhausted => Attempts >= MaxAttempts;
}
