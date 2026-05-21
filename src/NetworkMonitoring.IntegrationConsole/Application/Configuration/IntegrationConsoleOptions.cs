namespace NetworkMonitoring.IntegrationConsole.Application.Configuration;

/// <summary>
/// Configuration options for the Integration Console application.
/// </summary>
public sealed class IntegrationConsoleOptions
{
    /// <summary>
    /// The name of the configuration section.
    /// </summary>
    public const string SectionName = "IntegrationConsole";

    /// <summary>
    /// Gets the Kafka bootstrap servers.
    /// </summary>
    public string KafkaBootstrapServers { get; init; } = "localhost:9092,localhost:9093,localhost:9094";

    /// <summary>
    /// Gets the Schema Registry URL.
    /// </summary>
    public string SchemaRegistryUrl { get; init; } = "http://localhost:8081";

    /// <summary>
    /// Gets the Kafka topic for detected devices.
    /// </summary>
    public string KafkaDeviceTopic { get; init; } = "devices.detected";

    /// <summary>
    /// Gets the Kafka consumer group ID.
    /// </summary>
    public string KafkaConsumerGroupId { get; init; } = "device-ingestion-local";

    /// <summary>
    /// Gets the base URL for the Backend API.
    /// </summary>
    public string BackendBaseUrl { get; init; } = "http://localhost:5080";

    /// <summary>
    /// Gets the maximum number of retry attempts for failed operations.
    /// </summary>
    public int RetryMaxAttempts { get; init; } = 3;

    /// <summary>
    /// Gets the base delay in milliseconds for retries.
    /// </summary>
    public int RetryBaseDelayMilliseconds { get; init; } = 250;

    /// <summary>
    /// Gets the HTTP timeout in seconds.
    /// </summary>
    public int HttpTimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Gets the Kafka security protocol (e.g., Ssl).
    /// </summary>
    public string? KafkaSecurityProtocol { get; init; }

    /// <summary>
    /// Gets the path to the Kafka SSL CA certificate.
    /// </summary>
    public string? KafkaSslCaLocation { get; init; }

    /// <summary>
    /// Gets the path to the Kafka SSL certificate.
    /// </summary>
    public string? KafkaSslCertificateLocation { get; init; }

    /// <summary>
    /// Gets the path to the Kafka SSL key.
    /// </summary>
    public string? KafkaSslKeyLocation { get; init; }
}
