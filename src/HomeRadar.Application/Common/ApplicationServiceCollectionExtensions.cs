using HomeRadar.Application.Analysis;
using HomeRadar.Application.Ingestion;
using HomeRadar.Application.Notifications;
using HomeRadar.Application.Outbox;
using HomeRadar.Application.Seeding;
using Microsoft.Extensions.DependencyInjection;

namespace HomeRadar.Application.Common;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddHomeRadarApplication(this IServiceCollection services)
    {
        services.AddScoped<IIngestionStage, ChangeDetectionStage>();
        services.AddScoped<IIngestionStage, EnrichmentStage>();
        services.AddScoped<IIngestionStage, AnalysisStage>();
        services.AddScoped<IIngestionStage, MatchStage>();
        services.AddScoped<IIngestionStage, NotifyStage>();
        services.AddScoped<SourceIngestionService>();
        services.AddScoped<OutboxDispatcher>();
        services.AddScoped<IOutboxHandler, ListingMatchedHandler>();
        services.AddScoped<IOutboxHandler, WatchlistChangedHandler>();
        services.AddScoped<IOutboxHandler, SourceHealthHandler>();
        services.AddScoped<IOutboxHandler, ListingEventWebhookHandler>();
        services.AddScoped<SeedService>();
        services.AddScoped<FinanceConfigurationLoader>();
        services.AddScoped<AnalysisService>();
        services.AddScoped<AffordabilityLimitResolver>();
        return services;
    }
}
