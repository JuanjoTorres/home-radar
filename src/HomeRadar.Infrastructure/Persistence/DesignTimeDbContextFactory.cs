using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HomeRadar.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<HomeRadarDbContext>
{
    public HomeRadarDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("HOMERADAR_CONNECTION") ?? "Host=localhost;Username=homeradar;Password=homeradar;Database=homeradar";
        var options = new DbContextOptionsBuilder<HomeRadarDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite().MigrationsAssembly(typeof(HomeRadarDbContext).Assembly))
            .Options;
        return new HomeRadarDbContext(options);
    }
}
