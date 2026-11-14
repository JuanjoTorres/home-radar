using HomeRadar.Application.Ports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeRadar.Application.Outbox;

public sealed class OutboxDispatcher(IHomeRadarDbContext db, IEnumerable<IOutboxHandler> handlers, TimeProvider clock, ILogger<OutboxDispatcher> logger)
{
    private const int BatchSize = 50;

    private readonly Dictionary<string, IOutboxHandler> _handlers = handlers.ToDictionary(handler => handler.MessageType, StringComparer.Ordinal);

    public async Task<int> DispatchPendingAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var pending = await db.OutboxMessages
            .Where(message => message.ProcessedAt == null && (message.NextAttemptAt == null || message.NextAttemptAt <= now) && message.Attempts < Domain.Outbox.OutboxMessage.MaxAttempts)
            .OrderBy(message => message.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        var dispatched = 0;
        foreach (var message in pending)
        {
            if (!_handlers.TryGetValue(message.Type, out var handler))
            {
                message.MarkFailed(clock.GetUtcNow(), $"No handler registered for '{message.Type}'.");
                continue;
            }

            try
            {
                await handler.HandleAsync(message, cancellationToken);
                message.MarkProcessed(clock.GetUtcNow());
                dispatched++;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception, "Outbox message {MessageId} of type {Type} failed (attempt {Attempt})", message.Id, message.Type, message.Attempts + 1);
                message.MarkFailed(clock.GetUtcNow(), exception.Message);
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        return dispatched;
    }
}
