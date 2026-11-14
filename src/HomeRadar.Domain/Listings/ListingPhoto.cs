namespace HomeRadar.Domain.Listings;

public sealed class ListingPhoto
{
    private ListingPhoto()
    {
    }

    public ListingPhoto(Uri url, int position)
    {
        Url = url;
        Position = position;
    }

    public Guid Id { get; private init; } = Guid.CreateVersion7();

    public Uri Url { get; private set; } = null!;

    public int Position { get; private set; }

    public ulong? PerceptualHash { get; private set; }

    public Uri? StoredCopy { get; private set; }

    public void SetPerceptualHash(ulong hash) => PerceptualHash = hash;

    public void SetStoredCopy(Uri location) => StoredCopy = location;

    internal void MoveTo(int position) => Position = position;
}
