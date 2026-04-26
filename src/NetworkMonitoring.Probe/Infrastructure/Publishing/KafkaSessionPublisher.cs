using Avro.Generic;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Probe.Application.Configuration;
using NetworkMonitoring.Probe.Application.Ports;

namespace NetworkMonitoring.Probe.Infrastructure.Publishing;

public sealed class KafkaSessionPublisher : IMessagePublisher, IDisposable
{
    private readonly ProbeOptions _options;
    private readonly ILogger<KafkaSessionPublisher> _logger;
    private readonly object _gate = new();
    private CachedSchemaRegistryClient? _schemaRegistry;
    private IProducer<string, GenericRecord>? _producer;

    public KafkaSessionPublisher(IOptions<ProbeOptions> options, ILogger<KafkaSessionPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishSessionDetected(Session session, CancellationToken cancellationToken)
    {
        if (!_options.EnableKafka)
        {
            return;
        }

        try
        {
            EnsureProducer();
            var record = SessionDetectedAvroMapper.ToGenericRecord(session, DateTimeOffset.UtcNow);
            var key = SessionKafkaPartitionKey.Build(session);
            await _producer!.ProduceAsync(
                    _options.KafkaSessionTopic,
                    new Message<string, GenericRecord> { Key = key, Value = record },
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish SessionDetected to Kafka topic {Topic}", _options.KafkaSessionTopic);
        }
    }

    public Task PublishDeviceDetected(Device device, CancellationToken cancellationToken)
    {
        _ = device;
        return Task.CompletedTask;
    }

    private void EnsureProducer()
    {
        if (_producer is not null)
        {
            return;
        }

        lock (_gate)
        {
            if (_producer is not null)
            {
                return;
            }

            var srConfig = new SchemaRegistryConfig { Url = _options.SchemaRegistryUrl };
            _schemaRegistry = new CachedSchemaRegistryClient(srConfig);

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _options.KafkaBootstrapServers,
                Acks = Acks.All,
                EnableIdempotence = true,
                SecurityProtocol = ParseSecurityProtocol(_options.KafkaSecurityProtocol),
            };

            if (!string.IsNullOrWhiteSpace(_options.KafkaSslCaLocation))
            {
                producerConfig.SslCaLocation = _options.KafkaSslCaLocation;
            }

            if (!string.IsNullOrWhiteSpace(_options.KafkaSslCertificateLocation))
            {
                producerConfig.SslCertificateLocation = _options.KafkaSslCertificateLocation;
            }

            if (!string.IsNullOrWhiteSpace(_options.KafkaSslKeyLocation))
            {
                producerConfig.SslKeyLocation = _options.KafkaSslKeyLocation;
            }

            var avroConfig = new AvroSerializerConfig
            {
                SubjectNameStrategy = SubjectNameStrategy.Topic,
                AutoRegisterSchemas = true,
            };

            _producer = new ProducerBuilder<string, GenericRecord>(producerConfig)
                .SetValueSerializer(new AvroSerializer<GenericRecord>(_schemaRegistry, avroConfig))
                .Build();
        }
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

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
        _schemaRegistry?.Dispose();
        _producer = null;
        _schemaRegistry = null;
    }
}
