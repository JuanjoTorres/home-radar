using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HomeRadar.Api.IntegrationTests.Support;

public sealed class ApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    public const string ApiKey = "integration-test-api-key-0001";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:HomeRadar", connectionString);
        builder.UseSetting("HomeRadar:ApiKey", ApiKey);
        builder.UseSetting("HomeRadar:RequestsPerMinute", "1000");
        builder.UseSetting("Database:MigrateOnStartup", "false");
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", ApiKey);
        return client;
    }
}
