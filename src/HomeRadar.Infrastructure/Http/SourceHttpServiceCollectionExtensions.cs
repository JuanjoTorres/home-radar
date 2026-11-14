using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HomeRadar.Infrastructure.Http;

public static class SourceHttpServiceCollectionExtensions
{
    public const string ClientName = "homeradar-sources";

    public static IServiceCollection AddHomeRadarSourceHttp(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SourceHttpOptions>().Bind(configuration.GetSection(SourceHttpOptions.SectionName)).ValidateOnStart();
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<SourceHttpOptions>>().Value);
        services.AddSingleton<RobotsCache>();
        services.AddSingleton<PerHostRateLimitHandler>();
        services.AddTransient<RobotsTxtHandler>();

        services.AddHttpClient(ClientName, (provider, client) =>
            {
                var options = provider.GetRequiredService<SourceHttpOptions>();
                client.Timeout = options.Timeout;
                client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
                client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/json;q=0.9,*/*;q=0.5");
                client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("es-ES,es;q=0.9,ca;q=0.8");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 5,
                PooledConnectionLifetime = TimeSpan.FromMinutes(10),
            })
            .AddHttpMessageHandler<RobotsTxtHandler>()
            .AddHttpMessageHandler(provider => provider.GetRequiredService<PerHostRateLimitHandler>())
            .AddStandardResilienceHandler(resilience =>
            {
                resilience.Retry.MaxRetryAttempts = 3;
                resilience.Retry.Delay = TimeSpan.FromSeconds(5);
                resilience.AttemptTimeout.Timeout = TimeSpan.FromSeconds(60);
                resilience.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(5);
                resilience.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(2);
            });

        return services;
    }
}
