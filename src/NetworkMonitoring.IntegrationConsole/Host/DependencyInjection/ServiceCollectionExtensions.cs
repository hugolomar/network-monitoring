using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetworkMonitoring.IntegrationConsole.Application.Configuration;
using NetworkMonitoring.IntegrationConsole.Application.Ports;
using NetworkMonitoring.IntegrationConsole.Application.UseCases;
using NetworkMonitoring.IntegrationConsole.Host.Services;
using NetworkMonitoring.IntegrationConsole.Infrastructure.Backend;
using NetworkMonitoring.IntegrationConsole.Infrastructure.Ingestion;
using NetworkMonitoring.IntegrationConsole.Infrastructure.Observability;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace NetworkMonitoring.IntegrationConsole.Host.DependencyInjection;

/// <summary>
/// Extension methods for registering Integration Console services in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all necessary services for the Integration Console application.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>the updated service collection.</returns>
    public static IServiceCollection AddIntegrationConsole(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<IntegrationConsoleOptions>()
            .Bind(configuration.GetSection(IntegrationConsoleOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.KafkaDeviceTopic), "Kafka device topic is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.BackendBaseUrl), "Backend base URL is required.")
            .ValidateOnStart();

        services.AddSingleton(sp => RetryOptions.From(sp.GetRequiredService<IOptions<IntegrationConsoleOptions>>().Value));
        services.AddSingleton<IIngestionFlowTelemetry, IngestionFlowTelemetry>();
        services.AddSingleton<ProcessDeviceDetectionsUseCase>();
        services.AddSingleton<IDeviceEventConsumer, KafkaDeviceEventConsumer>();
        services.AddSingleton<DeviceIntakeRetryPolicy>();
        services.AddHttpClient<IDeviceIntakeClient, HttpDeviceIntakeClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<IntegrationConsoleOptions>>().Value;
            client.BaseAddress = new Uri(options.BackendBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(Math.Max(1, options.HttpTimeoutSeconds));
        });
        services.AddHealthChecks().AddCheck("integration-console-live", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["live", "ready"]);

        var enableStructuredLogging = configuration.GetValue<bool?>("Observability:EnableStructuredLogging") ?? true;
        var enableConsoleLogging = configuration.GetValue<bool?>("Observability:EnableConsoleLogging") ?? true;
        var enableOpenTelemetry = configuration.GetValue<bool?>("Observability:EnableOpenTelemetry") ?? true;
        var enableOtlpLogs = configuration.GetValue<bool?>("Observability:EnableOtlpLogs") ?? true;
        var configuredOtlpEndpoint = configuration["Observability:OtlpEndpoint"];
        var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
            ?? configuredOtlpEndpoint;

        if (enableStructuredLogging && enableConsoleLogging)
        {
            services.AddLogging(builder =>
            {
                builder.AddJsonConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.UseUtcTimestamp = true;
                });
            });
        }

        if (enableOpenTelemetry && !string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService("network-monitoring-integration-console"))
                .WithTracing(builder =>
                {
                    builder
                        .AddSource("NetworkMonitoring.IntegrationConsole")
                        .AddHttpClientInstrumentation()
                        .AddOtlpExporter(exporter => exporter.Endpoint = new Uri(otlpEndpoint));
                })
                .WithMetrics(builder =>
                {
                    builder
                        .AddMeter("NetworkMonitoring.IntegrationConsole")
                        .AddRuntimeInstrumentation()
                        .AddOtlpExporter(exporter => exporter.Endpoint = new Uri(otlpEndpoint));
                });
        }

        if (enableOpenTelemetry && enableOtlpLogs && !string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            services.AddLogging(builder =>
            {
                builder.AddOpenTelemetry(logging =>
                {
                    logging.IncludeScopes = true;
                    logging.IncludeFormattedMessage = true;
                    logging.ParseStateValues = true;
                    logging.AddOtlpExporter(exporter => exporter.Endpoint = new Uri(otlpEndpoint));
                });
            });
        }

        services.AddHostedService<IntegrationConsoleWorker>();

        return services;
    }
}
