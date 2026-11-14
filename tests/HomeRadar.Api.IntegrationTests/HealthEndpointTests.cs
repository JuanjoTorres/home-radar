using Microsoft.AspNetCore.Mvc.Testing;

namespace HomeRadar.Api.IntegrationTests;

public sealed class HealthEndpointTests
{
    [Fact]
    public async Task Liveness_endpoint_answers_200()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/health/live", UriKind.Relative), TestContext.Current.CancellationToken);

        response.IsSuccessStatusCode.ShouldBeTrue();
    }
}
