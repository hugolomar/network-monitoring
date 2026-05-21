namespace NetworkMonitoring.Probe.Application.Configuration;

/// <summary>
/// Holds configuration settings for the Probe application.
/// Maps to the "Probe" section in appsettings.json.
/// </summary>
public sealed class ProbeOptions
{
    /// <summary>
    /// The name of the configuration section.
    /// </summary>
    public const string SectionName = "Probe";

    /// <summary>
    /// The path to the tshark executable. Defaults to "tshark".
    /// </summary>
    public string TSharkPath { get; init; } = "tshark";

    /// <summary>
    /// The name of the network interface to listen on (e.g., "eth0").
    /// </summary>
    public string InterfaceName { get; init; } = "eth0";

    /// <summary>
    /// An optional libpcap filter expression to restrict captured traffic.
    /// </summary>
    public string CaptureFilter { get; init; } = string.Empty;

    /// <summary>
    /// The window in minutes for deduplicating repeated network sessions.
    /// </summary>
    public int SessionDeduplicationWindowMinutes { get; init; } = 10;

    /// <summary>
    /// The window in minutes for deduplicating repeated device detections.
    /// </summary>
    public int DeviceDeduplicationWindowMinutes { get; init; } = 10;

    /// <summary>
    /// Enables operator-visible console output for real-time monitoring.
    /// </summary>
    public bool EnableConsole { get; init; } = true;

    /// <summary>
    /// Enables publishing detection events to a Kafka topic.
    /// </summary>
    public bool EnableKafka { get; init; } = false;

    /// <summary>
    /// The list of Kafka bootstrap servers (Required if <see cref="EnableKafka"/> is true).
    /// </summary>
    public string? KafkaBootstrapServers { get; init; }

    /// <summary>
    /// The URL of the Confluent Schema Registry (Required if <see cref="EnableKafka"/> is true).
    /// </summary>
    public string? SchemaRegistryUrl { get; init; }

    /// <summary>
    /// The Kafka topic name for session detection events.
    /// </summary>
    public string KafkaSessionTopic { get; init; } = "sessions.detected";

    /// <summary>
    /// The Kafka topic name for device detection events.
    /// </summary>
    public string KafkaDeviceTopic { get; init; } = "devices.detected";

    /// <summary>
    /// The security protocol for Kafka (e.g., Plaintext, Ssl).
    /// </summary>
    public string? KafkaSecurityProtocol { get; init; }

    /// <summary>
    /// Path to the SSL CA certificate for Kafka.
    /// </summary>
    public string? KafkaSslCaLocation { get; init; }

    /// <summary>
    /// Path to the SSL client certificate for Kafka.
    /// </summary>
    public string? KafkaSslCertificateLocation { get; init; }

    /// <summary>
    /// Path to the SSL client private key for Kafka.
    /// </summary>
    public string? KafkaSslKeyLocation { get; init; }
}
