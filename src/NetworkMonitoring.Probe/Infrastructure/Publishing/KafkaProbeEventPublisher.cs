using Avro.Generic;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Probe.Application.Configuration;
using NetworkMonitoring.Probe.Application.Ports;

namespace NetworkMonitoring.Probe.Infrastructure.Publishing;

/// <summary>
/// Implementation of <see cref="IMessagePublisher"/> that sends detection events to Apache Kafka using Avro serialization.
/// Handles connection management and fault tolerance for the underlying Kafka producer.
/// </summary>
public sealed class KafkaProbeEventPublisher : IMessagePublisher, IDisposable
{
    private readonly ProbeOptions _options;
    private readonly ILogger<KafkaProbeEventPublisher> _logger;
    private readonly IKafkaGenericRecordProducerFactory _producerFactory;
    private readonly object _gate = new();
    private IKafkaGenericRecordProducer? _producer;

    /// <summary>
    /// Initializes a new instance of the <see cref="KafkaProbeEventPublisher"/> class.
    /// </summary>
    /// <param name="options">Configuration options for Kafka topics and servers.</param>
    /// <param name="logger">Logger for capturing publication errors and status.</param>
    /// <param name="producerFactory">Optional factory to customize producer creation (useful for testing).</param>
    public KafkaProbeEventPublisher(
        IOptions<ProbeOptions> options,
        ILogger<KafkaProbeEventPublisher> logger,
        IKafkaGenericRecordProducerFactory? producerFactory = null)
    {
        _options = options.Value;
        _logger = logger;
        _producerFactory = producerFactory ?? new KafkaGenericRecordProducerFactory();
    }

    /// <summary>
    /// Maps a session entity to an Avro record and publishes it to the configured Kafka topic.
    /// </summary>
    /// <param name="session">The session to publish.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous publication.</returns>
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

    /// <summary>
    /// Maps a device entity to an Avro record and publishes it to the configured Kafka topic.
    /// </summary>
    /// <param name="device">The device to publish.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous publication.</returns>
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

    /// <summary>
    /// Ensures the underlying Kafka producer is initialized.
    /// </summary>
    /// <remarks>
    /// Uses a Double-Check Locking pattern to ensure the producer is created only once 
    /// in a thread-safe manner without penalizing performance on subsequent calls.
    /// </remarks>
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

    /// <summary>
    /// Flushes any pending messages and releases the underlying producer resources.
    /// </summary>
    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
        _producer = null;
    }
}
