using System.ComponentModel.DataAnnotations;

namespace HomeRadar.Infrastructure.Notifications;

public sealed class TelegramOptions
{
    public const string SectionName = "Telegram";

    [Required]
    public string BotToken { get; set; } = string.Empty;

    [Required]
    public string ChatId { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;
}
