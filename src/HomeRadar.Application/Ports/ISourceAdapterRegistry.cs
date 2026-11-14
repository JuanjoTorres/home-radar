namespace HomeRadar.Application.Ports;

/// <summary>Resolves the <see cref="IListingSource"/> registered under an adapter key.</summary>
public interface ISourceAdapterRegistry
{
    /// <summary>Returns the adapter for the key.</summary>
    /// <param name="adapterKey">Value of <c>Source.AdapterKey</c>, e.g. <c>solvia</c> or <c>inmovilla</c>.</param>
    /// <exception cref="KeyNotFoundException">When no adapter is registered under the key.</exception>
    IListingSource Resolve(string adapterKey);

    /// <summary>All registered adapter keys, for diagnostics and validation of seed files.</summary>
    IReadOnlyCollection<string> Keys { get; }
}
