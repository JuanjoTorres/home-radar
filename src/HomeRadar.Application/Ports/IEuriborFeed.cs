namespace HomeRadar.Application.Ports;

/// <summary>Official monthly Euríbor 12M series.</summary>
public interface IEuriborFeed
{
    /// <summary>Gets the latest published monthly average.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The latest month with a value, or null when the series cannot be read.</returns>
    Task<EuriborObservation?> LatestAsync(CancellationToken cancellationToken);
}

/// <summary>Monthly Euríbor 12M average.</summary>
/// <param name="Month">First day of the month the average refers to.</param>
/// <param name="Percent">Average in %.</param>
/// <param name="Source">Where it was read.</param>
public sealed record EuriborObservation(DateOnly Month, decimal Percent, Uri Source);
