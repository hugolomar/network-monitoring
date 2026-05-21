using Avro.Generic;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using NetworkMonitoring.Probe.Application.Configuration;

namespace NetworkMonitoring.Probe.Infrastructure.Publishing;
/// <summary>
/// Defines a contract for a Kafka producer that handles Avro <see cref="GenericRecord"/> values.
/// </summary>
public interface IKafkaGenericRecordProducer : IDisposable
{
    /// <summary>
    /// Asynchronously sends a message to a Kafka topic.
    /// </summary>
    /// <param name="topic">The target Kafka topic.</param>
    /// <param name="message">The message to produce.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the production result.</returns>
    Task ProduceAsync(string topic, Message<string, GenericRecord> message, CancellationToken cancellationToken);

    /// <summary>
    /// Flushes all pending messages to the Kafka broker.
    /// </summary>
    /// <param name="timeout">The maximum time to wait for the flush to complete.</param>
    void Flush(TimeSpan timeout);
}

/// <summary>
/// Factory for creating <see cref="IKafkaGenericRecordProducer"/> instances.
/// </summary>
public interface IKafkaGenericRecordProducerFactory
{
    /// <summary>
    /// Creates a new configured Kafka producer.
    /// </summary>
    /// <param name="options">Configuration options for servers, security, and schema registry.</param>
    /// <returns>A configured <see cref="IKafkaGenericRecordProducer"/>.</returns>
    IKafkaGenericRecordProducer Create(ProbeOptions options);
}

/// <summary>
/// Standard implementation of the producer factory, integrating Confluent's Schema Registry and Avro serializers.
/// </summary>
public sealed class KafkaGenericRecordProducerFactory : IKafkaGenericRecordProducerFactory
{
    /// <summary>
    /// Configures and builds a Kafka producer with Avro serialization and Schema Registry integration.
    /// </summary>
    /// <param name="options">The probe configuration options.</param>
    /// <returns>A fully initialized <see cref="IKafkaGenericRecordProducer"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if required Kafka or Schema Registry settings are missing.</exception>
    public IKafkaGenericRecordProducer Create(ProbeOptions options)
...
    {
        if (string.IsNullOrWhiteSpace(options.KafkaBootstrapServers))
        {
            throw new InvalidOperationException("Probe:KafkaBootstrapServers is required when Kafka publishing is enabled.");
        }

        if (string.IsNullOrWhiteSpace(options.SchemaRegistryUrl))
        {
            throw new InvalidOperationException("Probe:SchemaRegistryUrl is required when Kafka publishing is enabled.");
        }

        var srConfig = new SchemaRegistryConfig { Url = options.SchemaRegistryUrl };
        var schemaRegistry = new CachedSchemaRegistryClient(srConfig);

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = options.KafkaBootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true,
            SecurityProtocol = ParseSecurityProtocol(options.KafkaSecurityProtocol),
        };

        if (!string.IsNullOrWhiteSpace(options.KafkaSslCaLocation))
        {
            producerConfig.SslCaLocation = options.KafkaSslCaLocation;
        }

        if (!string.IsNullOrWhiteSpace(options.KafkaSslCertificateLocation))
        {
            producerConfig.SslCertificateLocation = options.KafkaSslCertificateLocation;
        }

        if (!string.IsNullOrWhiteSpace(options.KafkaSslKeyLocation))
        {
            producerConfig.SslKeyLocation = options.KafkaSslKeyLocation;
        }

        var avroConfig = new AvroSerializerConfig
        {
            SubjectNameStrategy = SubjectNameStrategy.Topic,
            AutoRegisterSchemas = true,
        };

        var producer = new ProducerBuilder<string, GenericRecord>(producerConfig)
            .SetValueSerializer(new AvroSerializer<GenericRecord>(schemaRegistry, avroConfig))
            .Build();

        return new KafkaGenericRecordProducer(schemaRegistry, producer);
    }

    private static SecurityProtocol ParseSecurityProtocol(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return SecurityProtocol.Plaintext;
        }

        return Enum.TryParse<SecurityProtocol>(value, ignoreCase: true, out var protocol)
            ? protocol
            : SecurityProtocol.Plaintext;
    }
}

internal sealed class KafkaGenericRecordProducer(
    CachedSchemaRegistryClient schemaRegistry,
    IProducer<string, GenericRecord> producer) : IKafkaGenericRecordProducer
{
    public Task ProduceAsync(string topic, Message<string, GenericRecord> message, CancellationToken cancellationToken) =>
        producer.ProduceAsync(topic, message, cancellationToken);

    public void Flush(TimeSpan timeout) => producer.Flush(timeout);

    public void Dispose()
    {
        producer.Dispose();
        schemaRegistry.Dispose();
    }
}
