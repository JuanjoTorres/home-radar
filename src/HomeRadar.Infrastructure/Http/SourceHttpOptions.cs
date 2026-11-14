namespace HomeRadar.Infrastructure.Http;

public sealed class SourceHttpOptions
{
    public const string SectionName = "SourceHttp";

    public string UserAgent { get; set; } = "HomeRadarBot/0.1 (+https://github.com/JuanjoTorres/home-radar; personal, non-commercial)";

    public string UserAgentToken { get; set; } = "HomeRadarBot";

    public TimeSpan MinimumInterval { get; set; } = TimeSpan.FromSeconds(3);

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
}
