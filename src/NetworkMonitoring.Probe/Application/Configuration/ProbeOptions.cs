namespace NetworkMonitoring.Probe.Application.Configuration;

public sealed class ProbeOptions
{
    public const string SectionName = "Probe";

    public string TSharkPath { get; init; } = "tshark";

    public string InterfaceName { get; init; } = "eth0";

    public string CaptureFilter { get; init; } = string.Empty;

    public int SessionDeduplicationWindowMinutes { get; init; } = 10;

    public int DeviceDeduplicationWindowMinutes { get; init; } = 10;

    /// <summary>Operator-visible console output (US1).</summary>
    public bool EnableConsole { get; init; } = true;

    /// <summary>Kafka Avro publication (US2). Off by default for local capture-only runs.</summary>
    public bool EnableKafka { get; init; } = false;

    public string KafkaBootstrapServers { get; init; } = "localhost:9092,localhost:9093,localhost:9094";

    public string SchemaRegistryUrl { get; init; } = "http://localhost:8081";

    public string KafkaSessionTopic { get; init; } = "sessions.detected";

    /// <summary>e.g. Plaintext, Ssl, SaslSsl — see <see cref="Confluent.Kafka.SecurityProtocol"/>.</summary>
    public string? KafkaSecurityProtocol { get; init; }

    public string? KafkaSslCaLocation { get; init; }

    public string? KafkaSslCertificateLocation { get; init; }

    public string? KafkaSslKeyLocation { get; init; }
}
