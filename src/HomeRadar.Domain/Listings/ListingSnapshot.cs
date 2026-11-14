using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Domain.Listings;

public sealed class ListingSnapshot
{
    private ListingSnapshot()
    {
    }

    public ListingSnapshot(Guid listingId, Guid sourceRunId, DateTimeOffset capturedAt, ContentHash contentHash, string contentType, byte[] compressedBody)
    {
        ListingId = listingId;
        SourceRunId = sourceRunId;
        CapturedAt = capturedAt;
        ContentHash = contentHash;
        ContentType = contentType;
        CompressedBody = compressedBody;
    }

    public Guid Id { get; private init; } = Guid.CreateVersion7();

    public Guid ListingId { get; private init; }

    public Guid SourceRunId { get; private init; }

    public DateTimeOffset CapturedAt { get; private init; }

    public ContentHash ContentHash { get; private init; }

    public string ContentType { get; private init; } = "text/html";

    public byte[] CompressedBody { get; private init; } = [];
}
