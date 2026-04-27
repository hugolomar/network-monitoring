using Avro.Generic;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Probe.Application.Configuration;
using NetworkMonitoring.Probe.Application.Ports;

namespace NetworkMonitoring.Probe.Infrastructure.Publishing;

public sealed class KafkaProbeEventPublisher : IMessagePublisher, IDisposable
{
    private readonly ProbeOptions _options;
    private readonly ILogger<KafkaProbeEventPublisher> _logger;
    private readonly IKafkaGenericRecordProducerFactory _producerFactory;
    private readonly object _gate = new();
    private IKafkaGenericRecordProducer? _producer;

    public KafkaProbeEventPublisher(
        IOptions<ProbeOptions> options,
        ILogger<KafkaProbeEventPublisher> logger,
        IKafkaGenericRecordProducerFactory? producerFactory = null)
    {
        _options = options.Value;
        _logger = logger;
        _producerFactory = producerFactory ?? new KafkaGenericRecordProducerFactory();
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

    public async Task PublishDeviceDetected(Device device, CancellationToken cancellationToken)
    {
        if (!_options.EnableKafka)
        {
            return;
        }

        try
        {
            EnsureProducer();
            var record = DeviceDetectedAvroMapper.ToGenericRecord(device, DateTimeOffset.UtcNow);
            var key = DeviceKafkaPartitionKey.Build(device);
            await _producer!.ProduceAsync(
                    _options.KafkaDeviceTopic,
                    new Message<string, GenericRecord> { Key = key, Value = record },
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish DeviceDetected to Kafka topic {Topic}", _options.KafkaDeviceTopic);
        }
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

            _producer = _producerFactory.Create(_options);
        }
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
        _producer = null;
    }
}
