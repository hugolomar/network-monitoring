using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Xunit;

namespace NetworkMonitoring.IntegrationConsole.IntegrationTests;

public sealed class KafkaDeviceIngestionIntegrationTests
{
    [SkippableFact]
    public async Task Reference_stack_exposes_devices_detected_topic_and_schema_registry()
    {
        Skip.IfNot(
            string.Equals(Environment.GetEnvironmentVariable("RUN_KAFKA_INTEGRATION"), "1", StringComparison.Ordinal),
            "Set RUN_KAFKA_INTEGRATION=1 and start the reference Kafka stack to run this test.");

        using var admin = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? "localhost:9092,localhost:9093,localhost:9094"
        }).Build();
        using var schemaRegistry = new CachedSchemaRegistryClient(new SchemaRegistryConfig
        {
            Url = Environment.GetEnvironmentVariable("SCHEMA_REGISTRY_URL") ?? "http://localhost:8081"
        });

        var metadata = admin.GetMetadata("devices.detected", TimeSpan.FromSeconds(10));
        var schema = await schemaRegistry.GetLatestSchemaAsync("devices.detected-value");

        Assert.Contains(metadata.Topics, topic => topic.Topic == "devices.detected");
        Assert.Contains("DeviceDetected", schema.SchemaString);
    }
}
