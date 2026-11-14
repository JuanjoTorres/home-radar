using HomeRadar.Application.Common;
using HomeRadar.Infrastructure.Http;
using HomeRadar.Infrastructure.Notifications;
using HomeRadar.Infrastructure.Persistence;
using HomeRadar.Infrastructure.Scheduling;
using HomeRadar.Sources.Common;
using HomeRadar.Worker;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((services, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Quartz", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Polly", Serilog.Events.LogEventLevel.Error)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(new CompactJsonFormatter()));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHomeRadarPersistence(builder.Configuration.GetConnectionString("HomeRadar") ?? throw new InvalidOperationException("ConnectionStrings:HomeRadar is required."));
builder.Services.AddHomeRadarApplication();
builder.Services.AddHomeRadarSourceHttp(builder.Configuration);
builder.Services.AddHomeRadarSources();
builder.Services.AddHomeRadarNotifications(builder.Configuration);
builder.Services.AddHomeRadarScheduling(builder.Configuration);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddHttpClientInstrumentation().AddSource("Quartz").AddOtlpExporterIfConfigured(builder.Configuration))
    .WithMetrics(metrics => metrics.AddHttpClientInstrumentation().AddMeter("System.Runtime").AddOtlpExporterIfConfigured(builder.Configuration));

var host = builder.Build();

if (builder.Configuration.GetValue("Database:MigrateOnStartup", true))
{
    using var scope = host.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<HomeRadarDbContext>().Database.MigrateAsync();
}

await host.RunAsync();
