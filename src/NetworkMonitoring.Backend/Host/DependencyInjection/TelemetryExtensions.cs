using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using NetworkMonitoring.Backend.Application.Configuration;

namespace NetworkMonitoring.Backend.Host.DependencyInjection;

/// <summary>
/// Registers OpenTelemetry traces and metrics for backend services.
/// </summary>
internal static class TelemetryExtensions
{
    /// <summary>
    /// Adds backend OpenTelemetry pipeline.
    /// </summary>
    public static IServiceCollection AddBackendTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(ObservabilityOptions.SectionName).Get<ObservabilityOptions>() ?? new ObservabilityOptions();
        if (!options.EnableOpenTelemetry)
        {
            return services;
        }

        services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(serviceName: "network-monitoring-backend");
            })
            .WithTracing(builder =>
            {
                builder
                    .AddSource("NetworkMonitoring.Backend.Intake")
                    .AddSource("NetworkMonitoring.Backend.Alerting")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(exporter => exporter.Endpoint = new Uri(options.OtlpEndpoint));
            })
            .WithMetrics(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter("NetworkMonitoring.Backend.Graph")
                    .AddMeter("NetworkMonitoring.Backend.Alerting")
                    .AddMeter("NetworkMonitoring.Backend.Intake")
                    .AddOtlpExporter(exporter => exporter.Endpoint = new Uri(options.OtlpEndpoint));
            });

        return services;
    }
}
