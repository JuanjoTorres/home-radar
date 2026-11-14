using HomeRadar.Domain.Profiles;

namespace HomeRadar.Application.Ports;

/// <summary>Delivers a rendered notification through one channel (Telegram, email, webhook).</summary>
public interface INotificationChannel
{
    /// <summary>The channel this implementation serves.</summary>
    AlertChannel Channel { get; }

    /// <summary>
    /// Sends the notification. Must be idempotent per <paramref name="notification"/> id where the channel allows it.
    /// </summary>
    /// <param name="notification">Text, optional photo and link to deliver.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendAsync(Notification notification, CancellationToken cancellationToken);
}

public sealed record Notification(Guid Id, string Title, string Body, Uri? Link, Uri? PhotoUrl);
