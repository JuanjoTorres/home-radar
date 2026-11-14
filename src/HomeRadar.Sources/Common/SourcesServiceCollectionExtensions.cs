using HomeRadar.Application.Ports;
using HomeRadar.Sources.Aliseda;
using HomeRadar.Sources.Diglo;
using HomeRadar.Sources.Inmovilla;
using HomeRadar.Sources.Solvia;
using Microsoft.Extensions.DependencyInjection;

namespace HomeRadar.Sources.Common;

public static class SourcesServiceCollectionExtensions
{
    public static IServiceCollection AddHomeRadarSources(this IServiceCollection services)
    {
        services.AddSingleton<ISourceHttpClient, SourceHttpClient>();
        services.AddSingleton<IListingSource, SolviaSource>();
        services.AddSingleton<IListingSource, AlisedaSource>();
        services.AddSingleton<IListingSource, DigloSource>();
        services.AddSingleton<IListingSource, InmovillaNativeSource>();
        services.AddSingleton<ISourceAdapterRegistry, SourceAdapterRegistry>();
        return services;
    }
}
