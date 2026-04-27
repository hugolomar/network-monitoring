using Avro.Generic;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using NetworkMonitoring.Probe.Application.Configuration;

namespace NetworkMonitoring.Probe.Infrastructure.Publishing;

public interface IKafkaGenericRecordProducer : IDisposable
{
    Task ProduceAsync(string topic, Message<string, GenericRecord> message, CancellationToken cancellationToken);

    void Flush(TimeSpan timeout);
}

public interface IKafkaGenericRecordProducerFactory
{
    IKafkaGenericRecordProducer Create(ProbeOptions options);
}

public sealed class KafkaGenericRecordProducerFactory : IKafkaGenericRecordProducerFactory
{
    public IKafkaGenericRecordProducer Create(ProbeOptions options)
    {
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
