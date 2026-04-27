namespace NetworkMonitoring.IntegrationConsole.Application.Configuration;

public sealed class IntegrationConsoleOptions
{
    public const string SectionName = "IntegrationConsole";

    public string KafkaBootstrapServers { get; init; } = "localhost:9092,localhost:9093,localhost:9094";

    public string SchemaRegistryUrl { get; init; } = "http://localhost:8081";

    public string KafkaDeviceTopic { get; init; } = "devices.detected";

    public string KafkaConsumerGroupId { get; init; } = "device-ingestion-local";

    public string BackendBaseUrl { get; init; } = "http://localhost:5080";

    public int RetryMaxAttempts { get; init; } = 3;

    public int RetryBaseDelayMilliseconds { get; init; } = 250;

    public int HttpTimeoutSeconds { get; init; } = 30;

    public string? KafkaSecurityProtocol { get; init; }

    public string? KafkaSslCaLocation { get; init; }

    public string? KafkaSslCertificateLocation { get; init; }

    public string? KafkaSslKeyLocation { get; init; }
}
