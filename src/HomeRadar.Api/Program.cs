using System.Threading.RateLimiting;
using Asp.Versioning;
using HomeRadar.Api;
using HomeRadar.Api.Endpoints;
using HomeRadar.Api.Infrastructure;
using HomeRadar.Application.Common;
using HomeRadar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((services, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(new CompactJsonFormatter()));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddOptions<ApiOptions>().Bind(builder.Configuration.GetSection(ApiOptions.SectionName)).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddHomeRadarPersistence(builder.Configuration.GetConnectionString("HomeRadar") ?? throw new InvalidOperationException("ConnectionStrings:HomeRadar is required."));
builder.Services.AddHomeRadarApplication();
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
{
    context.ProblemDetails.Instance = context.HttpContext.Request.Path;
    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.Headers.RetryAfter = "60";
        await Results.Problem(statusCode: StatusCodes.Status429TooManyRequests, title: "Rate limit exceeded", type: ProblemTypes.RateLimited).ExecuteAsync(context.HttpContext);
    };
    options.AddPolicy("api", context =>
    {
        var limit = context.RequestServices.GetRequiredService<IOptions<ApiOptions>>().Value.RequestsPerMinute;
        var key = context.Request.Headers[ApiKeyMiddleware.HeaderName].ToString();
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions { PermitLimit = limit, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 });
    });
});
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer<HomeRadarDocumentTransformer>();
    options.AddSchemaTransformer<DecimalSchemaTransformer>();
    options.AddOperationTransformer<OperationConventionsTransformer>();
    options.AddOperationTransformer<OperationExamplesTransformer>();
});
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database");
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddOtlpExporterIfConfigured(builder.Configuration))
    .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation().AddOtlpExporterIfConfigured(builder.Configuration));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseRateLimiter();

app.MapOpenApi();
app.MapScalarApiReference(options => options.WithTitle("HomeRadar Mallorca API"));

app.MapHealthChecks("/health/live", new() { Predicate = _ => false }).WithName("getLiveness").WithTags("Health");
app.MapHealthChecks("/health/ready").WithName("getReadiness").WithTags("Health");

var versionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(1, 0)).ReportApiVersions().Build();
var v1 = app.MapGroup("/v{version:apiVersion}").WithApiVersionSet(versionSet).RequireRateLimiting("api");
v1.MapListingEndpoints();
v1.MapProfileEndpoints();
v1.MapWatchlistEndpoints();
v1.MapSourceEndpoints();
v1.MapAnalysisEndpoints();

if (builder.Configuration.GetValue("Database:MigrateOnStartup", false))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<HomeRadarDbContext>().Database.MigrateAsync();
}

await app.RunAsync();

public partial class Program;
