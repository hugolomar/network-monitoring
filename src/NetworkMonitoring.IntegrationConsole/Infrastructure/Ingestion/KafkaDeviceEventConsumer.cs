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

public sealed class KafkaDeviceEventConsumer : IDeviceEventConsumer
{
    private readonly IntegrationConsoleOptions _options;
    private readonly ILogger<KafkaDeviceEventConsumer> _logger;
    private readonly Lazy<IConsumer<string, GenericRecord>> _consumer;

    public KafkaDeviceEventConsumer(
        IOptions<IntegrationConsoleOptions> options,
        ILogger<KafkaDeviceEventConsumer> logger)
    {
        _options = options.Value;
        _logger = logger;
        _consumer = new Lazy<IConsumer<string, GenericRecord>>(CreateConsumer);
    }

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

    public Task Acknowledge(ConsumedDeviceEvent consumedEvent, CancellationToken cancellationToken)
    {
        if (_consumer.IsValueCreated && consumedEvent.Offset >= 0)
        {
            _consumer.Value.StoreOffset(new TopicPartitionOffset(
                consumedEvent.Topic,
                new Partition(consumedEvent.Partition),
                new Offset(consumedEvent.Offset + 1)));
        }

        return Task.CompletedTask;
    }

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
