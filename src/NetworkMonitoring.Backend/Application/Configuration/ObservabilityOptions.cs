namespace NetworkMonitoring.Backend.Application.Configuration;

/// <summary>
/// Represents observability-related host options.
/// </summary>
public sealed class ObservabilityOptions
{
    /// <summary>
    /// Configuration section name for observability options.
    /// </summary>
    public const string SectionName = "Observability";

    /// <summary>
    /// Gets a value indicating whether OpenTelemetry exporters are enabled.
    /// </summary>
    public bool EnableOpenTelemetry { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether structured logging enrichment is enabled.
    /// </summary>
    public bool EnableStructuredLogging { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether JSON console logging is enabled.
    /// </summary>
    public bool EnableConsoleLogging { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether OTLP log export is enabled.
    /// </summary>
    public bool EnableOtlpLogs { get; init; } = true;

    /// <summary>
    /// Gets the OTLP endpoint used by exporters.
    /// </summary>
    public string OtlpEndpoint { get; init; } = "http://localhost:4317";
}
