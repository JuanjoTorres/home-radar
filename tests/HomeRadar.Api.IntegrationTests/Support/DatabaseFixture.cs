using HomeRadar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace HomeRadar.Api.IntegrationTests.Support;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private const string ConnectionVariable = "HOMERADAR_TEST_CONNECTION";

    private readonly List<string> _createdDatabases = [];
    private PostgreSqlContainer? _container;

    public string AdminConnectionString { get; private set; } = string.Empty;

    public async ValueTask InitializeAsync()
    {
        var configured = Environment.GetEnvironmentVariable(ConnectionVariable);
        if (configured is { Length: > 0 })
        {
            AdminConnectionString = configured;
            return;
        }

        _container = new PostgreSqlBuilder("postgis/postgis:16-3.5").WithDatabase("homeradar_test").WithUsername("homeradar").WithPassword("homeradar").Build();
        await _container.StartAsync();
        AdminConnectionString = _container.GetConnectionString();
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
            return;
        }

        await using var connection = new NpgsqlConnection(AdminConnectionString);
        await connection.OpenAsync();
        foreach (var database in _createdDatabases)
        {
            await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{database}\" WITH (FORCE)", connection);
            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task<string> CreateDatabaseAsync(string prefix)
    {
        var name = $"{prefix}_{Guid.NewGuid():N}".ToLowerInvariant();
        var builder = new NpgsqlConnectionStringBuilder(AdminConnectionString);
        await using (var connection = new NpgsqlConnection(builder.ConnectionString))
        {
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", connection);
            await command.ExecuteNonQueryAsync();
        }

        _createdDatabases.Add(name);
        builder.Database = name;
        var options = BuildOptions(builder.ConnectionString);
        await using var db = new HomeRadarDbContext(options);
        await db.Database.MigrateAsync();
        return builder.ConnectionString;
    }

    public static DbContextOptions<HomeRadarDbContext> BuildOptions(string connectionString) =>
        new DbContextOptionsBuilder<HomeRadarDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite())
            .Options;
}

[CollectionDefinition(Name)]
public sealed class DatabaseTests : ICollectionFixture<DatabaseFixture>
{
    public const string Name = "database";
}
