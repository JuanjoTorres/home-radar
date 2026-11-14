using HomeRadar.Application.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeRadar.Infrastructure.Notifications;

public static class NotificationsServiceCollectionExtensions
{
    public static IServiceCollection AddHomeRadarNotifications(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<TelegramOptions>().Bind(configuration.GetSection(TelegramOptions.SectionName));
        services.AddSingleton<INotificationChannel, TelegramNotificationChannel>();
        return services;
    }
}
