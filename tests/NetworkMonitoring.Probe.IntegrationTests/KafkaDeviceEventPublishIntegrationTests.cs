using Avro.Generic;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Domain.ValueObjects;
using NetworkMonitoring.Probe.Application.Configuration;
using NetworkMonitoring.Probe.Infrastructure.Publishing;

namespace NetworkMonitoring.Probe.IntegrationTests;

/// <summary>
/// End-to-end produce + consume against a running stack. Skipped unless
/// <c>RUN_KAFKA_INTEGRATION=1</c> and brokers + Schema Registry are reachable (see quickstart).
/// </summary>
public sealed class KafkaDeviceEventPublishIntegrationTests
{
    [SkippableFact]
    public async Task PublishDeviceDetected_ProducesConsumableAvroValueWithNormalizedMacKey()
    {
        Skip.If(
            Environment.GetEnvironmentVariable("RUN_KAFKA_INTEGRATION") != "1",
            "Set RUN_KAFKA_INTEGRATION=1 with docker compose -f docker-compose.reference-stack.yml up and ./scripts/bootstrap/kafka-topics-init.sh.");

        var bootstrap = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS")
            ?? "localhost:9092,localhost:9093,localhost:9094";
        var registryUrl = Environment.GetEnvironmentVariable("SCHEMA_REGISTRY_URL")
            ?? "http://localhost:8081";
        var topic = Environment.GetEnvironmentVariable("KAFKA_DEVICE_TOPIC") ?? "devices.detected";

        var marker = $"device-{Guid.NewGuid():N}";
        var t = DateTimeOffset.UtcNow;
        var device = Device.Create(
            null,
            new MacAddress("02-42-ac-11-00-03"),
            new IpAddress("203.0.113.30"),
            marker,
            [new IpAddress("203.0.113.30")],
            t,
            t,
            DiscoverySource.FromRaw("traffic"));

        var options = Options.Create(
            new ProbeOptions
            {
                EnableConsole = false,
                EnableKafka = true,
                KafkaBootstrapServers = bootstrap,
                SchemaRegistryUrl = registryUrl,
                KafkaDeviceTopic = topic,
                KafkaSecurityProtocol = "Plaintext",
            });

        using (var publisher = new KafkaProbeEventPublisher(options, NullLogger<KafkaProbeEventPublisher>.Instance))
        {
            await publisher.PublishDeviceDetected(device, CancellationToken.None);
        }

        var srConfig = new SchemaRegistryConfig { Url = registryUrl };
        using var registry = new CachedSchemaRegistryClient(srConfig);

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrap,
            GroupId = $"probe-kafka-device-integration-{Guid.NewGuid():N}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };

        using var consumer = new ConsumerBuilder<string, GenericRecord>(consumerConfig)
            .SetValueDeserializer(new AvroDeserializer<GenericRecord>(registry).AsSyncOverAsync())
            .Build();

        consumer.Subscribe(topic);

        var deadline = DateTime.UtcNow.AddSeconds(60);
        ConsumeResult<string, GenericRecord>? found = null;
        while (found is null && DateTime.UtcNow < deadline)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            try
            {
                var result = consumer.Consume(cts.Token);
                if (result.Message.Value is null)
                {
                    continue;
                }

                if (Equals(result.Message.Value["hostname"], marker))
                {
                    found = result;
                }
            }
            catch (OperationCanceledException)
            {
                // continue polling until deadline
            }
        }

        Assert.NotNull(found);
        Assert.Equal("02:42:AC:11:00:03", found.Message.Key);
        Assert.Equal("DeviceDetected", found.Message.Value["eventType"]);
        Assert.Equal("02:42:AC:11:00:03", found.Message.Value["macAddress"]);
        Assert.Equal("203.0.113.30", found.Message.Value["primaryIp"]);
        Assert.Equal(marker, found.Message.Value["hostname"]);
        Assert.Equal("TRAFFIC", found.Message.Value["discoverySource"]);
    }
}
