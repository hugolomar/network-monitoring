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
public sealed class KafkaSessionPublishIntegrationTests
{
    [SkippableFact]
    public async Task PublishSessionDetected_ProducesConsumableAvroValue()
    {
        Skip.If(
            Environment.GetEnvironmentVariable("RUN_KAFKA_INTEGRATION") != "1",
            "Set RUN_KAFKA_INTEGRATION=1 with docker compose -f docker-compose.kafka.yml up and ./scripts/kafka-topics-init.sh.");

        var bootstrap = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS")
            ?? "localhost:9092,localhost:9093,localhost:9094";
        var registryUrl = Environment.GetEnvironmentVariable("SCHEMA_REGISTRY_URL")
            ?? "http://localhost:8081";
        var topic = Environment.GetEnvironmentVariable("KAFKA_SESSION_TOPIC") ?? "sessions.detected";

        var marker = Random.Shared.NextInt64(1_000_000_000_000, long.MaxValue);
        var t = DateTimeOffset.UtcNow;
        var session = Session.Create(
            null,
            new IpAddress("203.0.113.10"),
            new IpAddress("203.0.113.20"),
            new Port(40_000),
            new Port(40_001),
            ProtocolType.FromRaw("6"),
            t,
            t,
            marker);

        var options = Options.Create(
            new ProbeOptions
            {
                EnableKafka = true,
                KafkaBootstrapServers = bootstrap,
                SchemaRegistryUrl = registryUrl,
                KafkaSessionTopic = topic,
                KafkaSecurityProtocol = "Plaintext",
            });

        using (var publisher = new KafkaSessionPublisher(options, NullLogger<KafkaSessionPublisher>.Instance))
        {
            await publisher.PublishSessionDetected(session, CancellationToken.None);
        }

        var srConfig = new SchemaRegistryConfig { Url = registryUrl };
        using var registry = new CachedSchemaRegistryClient(srConfig);

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrap,
            GroupId = $"probe-kafka-integration-{Guid.NewGuid():N}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };

        using var consumer = new ConsumerBuilder<string, GenericRecord>(consumerConfig)
            .SetValueDeserializer(new AvroDeserializer<GenericRecord>(registry).AsSyncOverAsync())
            .Build();

        consumer.Subscribe(topic);

        var deadline = DateTime.UtcNow.AddSeconds(60);
        GenericRecord? found = null;
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

                if (result.Message.Value["bytesObserved"] is long l && l == marker)
                {
                    found = result.Message.Value;
                }
            }
            catch (OperationCanceledException)
            {
                // continue polling until deadline
            }
        }

        Assert.NotNull(found);
        Assert.Equal("SessionDetected", found["eventType"]);
        Assert.Equal("203.0.113.10", found["sourceIp"]);
        Assert.Equal("203.0.113.20", found["destinationIp"]);
        Assert.Equal(40_000, found["sourcePort"]);
        Assert.Equal(40_001, found["destinationPort"]);
        Assert.Equal("TCP", found["protocol"]);
    }
}
