using Avro.Generic;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetworkMonitoring.IntegrationConsole.Application.Configuration;
using NetworkMonitoring.IntegrationConsole.Application.Models;
using NetworkMonitoring.IntegrationConsole.Application.Ports;
using NetworkMonitoring.IntegrationConsole.Infrastructure.Serialization;
using System.Text;

namespace NetworkMonitoring.IntegrationConsole.Infrastructure.Ingestion;

/// <summary>
/// Kafka implementation of the device event consumer.
/// </summary>
public sealed class KafkaDeviceEventConsumer : IDeviceEventConsumer
{
    private readonly IntegrationConsoleOptions _options;
    private readonly ILogger<KafkaDeviceEventConsumer> _logger;
    private readonly Lazy<IConsumer<string, GenericRecord>> _consumer;

    /// <summary>
    /// Initializes a new instance of the <see cref="KafkaDeviceEventConsumer"/> class.
    /// </summary>
    /// <param name="options">Integration console configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public KafkaDeviceEventConsumer(
        IOptions<IntegrationConsoleOptions> options,
        ILogger<KafkaDeviceEventConsumer> logger)
    {
        _options = options.Value;
        _logger = logger;
        _consumer = new Lazy<IConsumer<string, GenericRecord>>(CreateConsumer);
    }

    /// <summary>
    /// Consumes device detected events from the Kafka topic.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An asynchronous stream of consumed device events.</returns>
    public async IAsyncEnumerable<ConsumedDeviceEvent> Consume(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var consumer = _consumer.Value;
        consumer.Subscribe(_options.KafkaDeviceTopic);

        while (!cancellationToken.IsCancellationRequested)
        {
            ConsumeResult<string, GenericRecord>? result = null;
            ConsumedDeviceEvent consumedEvent;

            try
            {
                result = consumer.Consume(cancellationToken);
                consumedEvent = new ConsumedDeviceEvent(
                    result.Message.Key,
                    DeviceDetectedEventMapper.FromGenericRecord(result.Message.Value),
                    result.Topic,
                    result.Partition.Value,
                    result.Offset.Value);
            }
            catch (ConsumeException ex)
            {
                // We handle ConsumeException separately to capture "poison pill" records 
                // that fail Kafka-level deserialization or broker-level issues.
                _logger.LogWarning(ex, "Failed to consume DeviceDetected event from Kafka");
                var poisonRecord = ex.ConsumerRecord;
                consumedEvent = new ConsumedDeviceEvent(
                    result?.Message.Key ?? TryDecodeUtf8(poisonRecord?.Message?.Key),
                    null,
                    result?.Topic ?? poisonRecord?.Topic ?? _options.KafkaDeviceTopic,
                    result?.Partition.Value ?? poisonRecord?.Partition.Value ?? 0,
                    result?.Offset.Value ?? poisonRecord?.Offset.Value ?? -1,
                    ex.Error.Reason);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }
            catch (Exception ex)
            {
                // General exception handling for mapping errors after successful consumption.
                _logger.LogWarning(ex, "Failed to decode DeviceDetected event from Kafka");
                consumedEvent = new ConsumedDeviceEvent(
                    result?.Message.Key,
                    null,
                    result?.Topic ?? _options.KafkaDeviceTopic,
                    result?.Partition.Value ?? 0,
                    result?.Offset.Value ?? -1,
                    ex.Message);
            }

            yield return consumedEvent;
            await Task.Yield();
        }
    }

    /// <summary>
    /// Acknowledges the successful processing of a consumed event by storing the offset.
    /// </summary>
    /// <param name="consumedEvent">The event to acknowledge.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task Acknowledge(ConsumedDeviceEvent consumedEvent, CancellationToken cancellationToken)
    {
        if (_consumer.IsValueCreated && consumedEvent.Offset >= 0)
        {
            // We use manual offset storage with auto-commit enabled. 
            // This ensures that we only commit offsets for messages that have been 
            // successfully processed or explicitly handled as failures.
            _consumer.Value.StoreOffset(new TopicPartitionOffset(
                consumedEvent.Topic,
                new Partition(consumedEvent.Partition),
                new Offset(consumedEvent.Offset + 1)));
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Closes and disposes of the Kafka consumer.
    /// </summary>
    /// <returns>A value task representing the asynchronous operation.</returns>
    public ValueTask DisposeAsync()
    {
        if (_consumer.IsValueCreated)
        {
            _consumer.Value.Close();
            _consumer.Value.Dispose();
        }

        return ValueTask.CompletedTask;
    }

    private IConsumer<string, GenericRecord> CreateConsumer()
    {
        // Kafka consumer configuration:
        // - EnableAutoCommit: true (let the background thread handle periodic commits)
        // - EnableAutoOffsetStore: false (we manually control when a message is "done" via StoreOffset)
        // - AutoOffsetReset: Earliest (ensure we don't miss data if the group is new)
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _options.KafkaBootstrapServers,
            GroupId = _options.KafkaConsumerGroupId,
            EnableAutoCommit = true,
            EnableAutoOffsetStore = false,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            SecurityProtocol = ParseSecurityProtocol(_options.KafkaSecurityProtocol)
        };

        consumerConfig.SslCaLocation = _options.KafkaSslCaLocation;
        consumerConfig.SslCertificateLocation = _options.KafkaSslCertificateLocation;
        consumerConfig.SslKeyLocation = _options.KafkaSslKeyLocation;

        var schemaRegistry = new CachedSchemaRegistryClient(new SchemaRegistryConfig
        {
            Url = _options.SchemaRegistryUrl
        });

        // Using Avro for schema-safe deserialization of events.
        var avroDeserializer = new AvroDeserializer<GenericRecord>(schemaRegistry).AsSyncOverAsync();

        return new ConsumerBuilder<string, GenericRecord>(consumerConfig)
            .SetValueDeserializer(avroDeserializer)
            .Build();
    }

    private static SecurityProtocol ParseSecurityProtocol(string? value) =>
        Enum.TryParse<SecurityProtocol>(value, ignoreCase: true, out var protocol)
            ? protocol
            : SecurityProtocol.Plaintext;

    private static string? TryDecodeUtf8(byte[]? value)
    {
        if (value is null || value.Length == 0)
        {
            return null;
        }

        try
        {
            return Encoding.UTF8.GetString(value);
        }
        catch (DecoderFallbackException)
        {
            return null;
        }
    }
}
