using HomeRadar.Application.Ports;
using HomeRadar.Domain.Profiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HomeRadar.Infrastructure.Notifications;

public sealed partial class TelegramNotificationChannel(IOptions<TelegramOptions> options, ILogger<TelegramNotificationChannel> logger) : INotificationChannel
{
    private const int CaptionLimit = 1000;
    private const int MessageLimit = 4000;

    private readonly Lazy<ITelegramBotClient> _client = new(() => new TelegramBotClient(options.Value.BotToken));

    public AlertChannel Channel => AlertChannel.Telegram;

    public async Task SendAsync(Notification notification, CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled || string.IsNullOrWhiteSpace(options.Value.BotToken))
        {
            Skipped(logger, notification.Id);
            return;
        }

        var chat = new ChatId(options.Value.ChatId);
        var text = notification.Body.Length > MessageLimit ? notification.Body[..MessageLimit] : notification.Body;

        if (notification.PhotoUrl is not null && text.Length <= CaptionLimit)
        {
            try
            {
                await _client.Value.SendPhoto(chat, InputFile.FromUri(notification.PhotoUrl), caption: text, parseMode: ParseMode.None, cancellationToken: cancellationToken);
                return;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                PhotoFailed(logger, exception, notification.Id);
            }
        }

        await _client.Value.SendMessage(chat, text, parseMode: ParseMode.None, linkPreviewOptions: new LinkPreviewOptions { IsDisabled = notification.PhotoUrl is not null }, cancellationToken: cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Telegram disabled; notification {NotificationId} skipped")]
    private static partial void Skipped(ILogger logger, Guid notificationId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Photo for notification {NotificationId} could not be sent; falling back to text")]
    private static partial void PhotoFailed(ILogger logger, Exception exception, Guid notificationId);
}
