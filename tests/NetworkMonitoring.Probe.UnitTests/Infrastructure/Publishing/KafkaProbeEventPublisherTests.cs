using Avro.Generic;
using Confluent.Kafka;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Domain.ValueObjects;
using NetworkMonitoring.Probe.Application.Configuration;
using NetworkMonitoring.Probe.Infrastructure.Publishing;

namespace NetworkMonitoring.Probe.UnitTests.Infrastructure.Publishing;

public sealed class KafkaProbeEventPublisherTests
{
    [Fact]
    public async Task PublishSessionDetected_WhenKafkaDisabled_DoesNotCreateProducer()
    {
        var factory = new CapturingProducerFactory(new CapturingProducer());
        using var publisher = CreatePublisher(new ProbeOptions { EnableKafka = false }, factory);

        await publisher.PublishSessionDetected(CreateSession(), CancellationToken.None);

        Assert.False(factory.WasCreated);
    }

    [Fact]
    public async Task PublishSessionDetected_ProducesToSessionTopicWithSessionKey()
    {
        var producer = new CapturingProducer();
        var factory = new CapturingProducerFactory(producer);
        using var publisher = CreatePublisher(
            new ProbeOptions
            {
                EnableKafka = true,
                KafkaSessionTopic = "sessions.detected",
            },
            factory);

        await publisher.PublishSessionDetected(CreateSession(), CancellationToken.None);

        var produced = Assert.Single(producer.Messages);
        Assert.Equal("sessions.detected", produced.Topic);
        Assert.Equal("10.0.0.1|10.0.0.2|1200|443|TCP", produced.Message.Key);
        Assert.Equal("SessionDetected", produced.Message.Value["eventType"]);
        Assert.Equal("10.0.0.1", produced.Message.Value["sourceIp"]);
        Assert.Equal("10.0.0.2", produced.Message.Value["destinationIp"]);
    }

    [Fact]
    public async Task PublishDeviceDetected_WhenKafkaDisabled_DoesNotCreateProducer()
    {
        var factory = new CapturingProducerFactory(new CapturingProducer());
        using var publisher = CreatePublisher(new ProbeOptions { EnableKafka = false }, factory);

        await publisher.PublishDeviceDetected(CreateDevice(), CancellationToken.None);

        Assert.False(factory.WasCreated);
    }

    [Fact]
    public async Task PublishDeviceDetected_WhenConsoleDisabled_ProducesToDeviceTopicWithNormalizedMacKey()
    {
        var producer = new CapturingProducer();
        var factory = new CapturingProducerFactory(producer);
        using var publisher = CreatePublisher(
            new ProbeOptions
            {
                EnableConsole = false,
                EnableKafka = true,
                KafkaDeviceTopic = "devices.detected",
            },
            factory);

        await publisher.PublishDeviceDetected(CreateDevice(), CancellationToken.None);

        var produced = Assert.Single(producer.Messages);
        Assert.Equal("devices.detected", produced.Topic);
        Assert.Equal("AA:BB:CC:DD:EE:FF", produced.Message.Key);
        Assert.Equal("DeviceDetected", produced.Message.Value["eventType"]);
        Assert.Equal("AA:BB:CC:DD:EE:FF", produced.Message.Value["macAddress"]);
    }

    [Fact]
    public async Task PublishDeviceDetected_WhenKafkaProduceFails_LogsAndDoesNotThrow()
    {
        var producer = new CapturingProducer { ThrowOnProduce = true };
        var factory = new CapturingProducerFactory(producer);
        using var publisher = CreatePublisher(new ProbeOptions { EnableKafka = true }, factory);

        var exception = await Record.ExceptionAsync(() => publisher.PublishDeviceDetected(CreateDevice(), CancellationToken.None));

        Assert.Null(exception);
    }

    private static KafkaProbeEventPublisher CreatePublisher(ProbeOptions options, IKafkaGenericRecordProducerFactory factory) =>
        new(Options.Create(options), NullLogger<KafkaProbeEventPublisher>.Instance, factory);

    private static Session CreateSession()
    {
        var t = DateTimeOffset.UtcNow;
        return Session.Create(
            null,
            new IpAddress("10.0.0.1"),
            new IpAddress("10.0.0.2"),
            new Port(1200),
            new Port(443),
            ProtocolType.FromRaw("6"),
            t,
            t,
            2048);
    }

    private static Device CreateDevice()
    {
        var t = DateTimeOffset.UtcNow;
        return Device.Create(
            null,
            new MacAddress("aa-bb-cc-dd-ee-ff"),
            new IpAddress("192.168.1.20"),
            "device-01",
            [new IpAddress("192.168.1.20")],
            t,
            t,
            DiscoverySource.FromRaw("traffic"));
    }

    private sealed class CapturingProducerFactory(IKafkaGenericRecordProducer producer) : IKafkaGenericRecordProducerFactory
    {
        public bool WasCreated { get; private set; }

        public IKafkaGenericRecordProducer Create(ProbeOptions options)
        {
            _ = options;
            WasCreated = true;
            return producer;
        }
    }

    private sealed class CapturingProducer : IKafkaGenericRecordProducer
    {
        public List<(string Topic, Message<string, GenericRecord> Message)> Messages { get; } = [];

        public bool ThrowOnProduce { get; init; }

        public Task ProduceAsync(string topic, Message<string, GenericRecord> message, CancellationToken cancellationToken)
        {
            _ = cancellationToken;
            if (ThrowOnProduce)
            {
                throw new InvalidOperationException("Simulated Kafka failure.");
            }

            Messages.Add((topic, message));
            return Task.CompletedTask;
        }

        public void Flush(TimeSpan timeout) => _ = timeout;

        public void Dispose()
        {
        }
    }
}
