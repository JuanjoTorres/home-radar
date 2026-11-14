namespace HomeRadar.Domain.Tests;

public sealed class PlaceholderTests
{
    [Fact]
    public void Domain_assembly_loads() => typeof(Domain.Common.Entity<>).Assembly.ShouldNotBeNull();
}
