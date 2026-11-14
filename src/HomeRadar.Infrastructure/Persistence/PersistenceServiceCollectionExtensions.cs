using HomeRadar.Application.Ports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HomeRadar.Infrastructure.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddHomeRadarPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<HomeRadarDbContext>(options => options
            .UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite().EnableRetryOnFailure(3)));
        services.AddScoped<IHomeRadarDbContext>(provider => provider.GetRequiredService<HomeRadarDbContext>());
        return services;
    }
}
