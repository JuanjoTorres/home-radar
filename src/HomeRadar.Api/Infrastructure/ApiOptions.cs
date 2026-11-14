using System.ComponentModel.DataAnnotations;

namespace HomeRadar.Api.Infrastructure;

public sealed class ApiOptions
{
    public const string SectionName = "HomeRadar";

    [Required]
    [MinLength(16)]
    public string ApiKey { get; set; } = string.Empty;

    public int RequestsPerMinute { get; set; } = 120;

    public TimeSpan IdempotencyLifetime { get; set; } = TimeSpan.FromHours(24);
}
