namespace HomeRadar.Domain.Outbox;

public sealed class IdempotencyRecord
{
    private IdempotencyRecord()
    {
    }

    public IdempotencyRecord(string key, string requestHash, int responseStatus, string responseBody, DateTimeOffset createdAt, TimeSpan lifetime)
    {
        Key = key;
        RequestHash = requestHash;
        ResponseStatus = responseStatus;
        ResponseBody = responseBody;
        CreatedAt = createdAt;
        ExpiresAt = createdAt + lifetime;
    }

    public string Key { get; private init; } = string.Empty;

    public string RequestHash { get; private init; } = string.Empty;

    public int ResponseStatus { get; private init; }

    public string ResponseBody { get; private init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private init; }

    public DateTimeOffset ExpiresAt { get; private init; }
}
