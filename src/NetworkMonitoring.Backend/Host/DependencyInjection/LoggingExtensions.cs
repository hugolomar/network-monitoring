using NetworkMonitoring.Backend.Application.Configuration;
using NetworkMonitoring.Backend.Host.Telemetry;
using NetworkMonitoring.Domain.Abstractions;
using OpenTelemetry.Logs;
using System.Diagnostics;

namespace NetworkMonitoring.Backend.Host.DependencyInjection;

/// <summary>
/// Registers backend logging enrichments required by observability baseline.
/// </summary>
internal static class LoggingExtensions
{
    /// <summary>
    /// Adds logging enrichments for correlation and service metadata.
    /// </summary>
    public static IServiceCollection AddBackendLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var observabilitySection = configuration.GetSection(ObservabilityOptions.SectionName);
        var options = observabilitySection.Get<ObservabilityOptions>() ?? new ObservabilityOptions();

        services.AddOptions<ObservabilityOptions>()
            .Bind(configuration.GetSection(ObservabilityOptions.SectionName))
            .ValidateOnStart();

        services.AddHttpContextAccessor();
        services.AddScoped<ICorrelationContext, HttpCorrelationContext>();

        services.Configure<LoggerFactoryOptions>(loggerOptions =>
        {
            loggerOptions.ActivityTrackingOptions =
                ActivityTrackingOptions.TraceId |
                ActivityTrackingOptions.SpanId |
                ActivityTrackingOptions.ParentId |
                ActivityTrackingOptions.Tags;
        });

        if (options.EnableStructuredLogging && options.EnableConsoleLogging)
        {
            services.AddLogging(builder =>
            {
                builder.AddJsonConsole(consoleOptions =>
                {
                    consoleOptions.IncludeScopes = true;
                    consoleOptions.UseUtcTimestamp = true;
                });
            });
        }

        if (options.EnableOpenTelemetry && options.EnableOtlpLogs && !string.IsNullOrWhiteSpace(options.OtlpEndpoint))
        {
            services.AddLogging(builder =>
            {
                builder.AddOpenTelemetry(logging =>
                {
                    logging.IncludeScopes = true;
                    logging.IncludeFormattedMessage = true;
                    logging.ParseStateValues = true;
                    logging.AddOtlpExporter(exporter => exporter.Endpoint = new Uri(options.OtlpEndpoint));
                });
            });
        }

        return services;
    }
}
