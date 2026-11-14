namespace HomeRadar.Sources.ContractTests;

public sealed class PlaceholderTests
{
    [Fact]
    public void Sources_assembly_loads() => typeof(Sources.Common.SourcesAssembly).Assembly.ShouldNotBeNull();
}
