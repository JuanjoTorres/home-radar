namespace HomeRadar.Application.Ports;

/// <summary>Computes a 64-bit perceptual hash of a photo so replaced images can be detected even when the URL is reused.</summary>
public interface IPhotoHasher
{
    /// <summary>Downloads and hashes the photo.</summary>
    /// <param name="photoUrl">Absolute URL of the image.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The hash, or <c>null</c> when the image could not be fetched or decoded.</returns>
    Task<ulong?> HashAsync(Uri photoUrl, CancellationToken cancellationToken);

    /// <summary>Hamming distance between two hashes; values below 10 usually mean the same picture.</summary>
    static int Distance(ulong left, ulong right) => System.Numerics.BitOperations.PopCount(left ^ right);
}
