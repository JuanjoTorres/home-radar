namespace HomeRadar.Application.Tests;

public sealed class PlaceholderTests
{
    [Fact]
    public void Application_assembly_loads() => typeof(Application.Common.IApplicationMarker).Assembly.ShouldNotBeNull();
}
