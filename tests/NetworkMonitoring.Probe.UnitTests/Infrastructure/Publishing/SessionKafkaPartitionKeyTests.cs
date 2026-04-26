using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Domain.ValueObjects;
using NetworkMonitoring.Probe.Infrastructure.Publishing;

namespace NetworkMonitoring.Probe.UnitTests.Infrastructure.Publishing;

public sealed class SessionKafkaPartitionKeyTests
{
    [Fact]
    public void Build_UsesSameFieldOrderAsSessionDeduplicationIdentity()
    {
        var session = Session.Create(
            null,
            new IpAddress("10.0.0.1"),
            new IpAddress("10.0.0.2"),
            new Port(1200),
            new Port(443),
            ProtocolType.FromRaw("6"),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            128);

        Assert.Equal("10.0.0.1|10.0.0.2|1200|443|TCP", SessionKafkaPartitionKey.Build(session));
    }

    [Fact]
    public void Build_WithNullPorts_UsesEmptySegmentsBetweenDelimiters()
    {
        var session = Session.Create(
            null,
            new IpAddress("192.168.1.1"),
            new IpAddress("192.168.1.2"),
            null,
            null,
            ProtocolType.FromRaw("17"),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            0);

        Assert.Equal("192.168.1.1|192.168.1.2|||UDP", SessionKafkaPartitionKey.Build(session));
    }

    [Fact]
    public void BuildUtf8Bytes_RoundTripsWithUtf8()
    {
        var session = Session.Create(
            null,
            new IpAddress("10.0.0.1"),
            new IpAddress("10.0.0.2"),
            new Port(1),
            new Port(2),
            ProtocolType.FromRaw("6"),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            1);

        var bytes = SessionKafkaPartitionKey.BuildUtf8Bytes(session);
        Assert.Equal(SessionKafkaPartitionKey.Build(session), System.Text.Encoding.UTF8.GetString(bytes));
    }
}
