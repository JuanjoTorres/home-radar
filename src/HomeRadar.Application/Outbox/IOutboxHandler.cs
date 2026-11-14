using HomeRadar.Domain.Outbox;

namespace HomeRadar.Application.Outbox;

/// <summary>Handles one outbox message type. Handlers must be idempotent: a message can be delivered more than once.</summary>
public interface IOutboxHandler
{
    /// <summary>The <see cref="OutboxMessage.Type"/> this handler consumes.</summary>
    string MessageType { get; }

    /// <summary>Performs the side effect (send a Telegram message, call a webhook).</summary>
    /// <param name="message">The message to process.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task HandleAsync(OutboxMessage message, CancellationToken cancellationToken);
}
