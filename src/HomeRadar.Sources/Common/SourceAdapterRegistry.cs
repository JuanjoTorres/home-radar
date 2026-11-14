using HomeRadar.Application.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace HomeRadar.Sources.Common;

public sealed class SourceAdapterRegistry(IServiceProvider services, IEnumerable<IListingSource> adapters) : ISourceAdapterRegistry
{
    private readonly Dictionary<string, IListingSource> _adapters = adapters.ToDictionary(adapter => adapter.AdapterKey, StringComparer.Ordinal);

    public IReadOnlyCollection<string> Keys => _adapters.Keys;

    public IListingSource Resolve(string adapterKey) =>
        _adapters.TryGetValue(adapterKey, out var adapter)
            ? adapter
            : services.GetKeyedService<IListingSource>(adapterKey) ?? throw new KeyNotFoundException($"No source adapter registered for '{adapterKey}'.");
}
