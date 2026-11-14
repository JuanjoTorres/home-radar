using System.Text.Json;
using HomeRadar.Domain.Common;

namespace HomeRadar.Domain.Listings;

public sealed class ListingEvent : IDomainEvent
{
    private ListingEvent()
    {
    }

    private ListingEvent(Guid listingId, ListingEventType type, DateTimeOffset occurredAt, Guid? snapshotId, object payload)
    {
        ListingId = listingId;
        Type = type;
        OccurredAt = occurredAt;
        SnapshotId = snapshotId;
        Payload = JsonSerializer.Serialize(payload, PayloadSerialization.Options);
    }

    public Guid Id { get; private init; } = Guid.CreateVersion7();

    public Guid ListingId { get; private init; }

    public ListingEventType Type { get; private init; }

    public DateTimeOffset OccurredAt { get; private init; }

    public Guid? SnapshotId { get; private set; }

    public string Payload { get; private init; } = "{}";

    public static ListingEvent Create(Guid listingId, ListingEventType type, DateTimeOffset occurredAt, object payload) =>
        new(listingId, type, occurredAt, null, payload);

    public void AttachSnapshot(Guid snapshotId) => SnapshotId = snapshotId;

    public T ReadPayload<T>() => JsonSerializer.Deserialize<T>(Payload, PayloadSerialization.Options)
        ?? throw new InvalidOperationException($"Event {Id} has no payload of type {typeof(T).Name}.");
}

public static class PayloadSerialization
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
    };
}
