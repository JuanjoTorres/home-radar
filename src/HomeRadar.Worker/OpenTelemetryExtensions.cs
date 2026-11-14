using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace HomeRadar.Worker;

internal static class OpenTelemetryExtensions
{
    private const string EndpointKey = "OTEL_EXPORTER_OTLP_ENDPOINT";

    public static TracerProviderBuilder AddOtlpExporterIfConfigured(this TracerProviderBuilder tracing, IConfiguration configuration) =>
        string.IsNullOrWhiteSpace(configuration[EndpointKey]) ? tracing : tracing.AddOtlpExporter();

    public static MeterProviderBuilder AddOtlpExporterIfConfigured(this MeterProviderBuilder metrics, IConfiguration configuration) =>
        string.IsNullOrWhiteSpace(configuration[EndpointKey]) ? metrics : metrics.AddOtlpExporter();
}
