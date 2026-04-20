using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Domain.ValueObjects;
using NetworkMonitoring.Probe.Infrastructure.Publishing;

namespace NetworkMonitoring.Probe.UnitTests.Infrastructure.Publishing;

public sealed class SessionDetectedAvroMapperTests
{
    [Fact]
    public void ToGenericRecord_MapsAllContractFields()
    {
        var first = new DateTimeOffset(2025, 3, 15, 10, 30, 0, TimeSpan.Zero);
        var last = new DateTimeOffset(2025, 3, 15, 10, 31, 5, TimeSpan.Zero);
        var occurred = new DateTimeOffset(2025, 3, 15, 10, 32, 0, TimeSpan.Zero);

        var session = Session.Create(
            99,
            new IpAddress("10.0.0.1"),
            new IpAddress("10.0.0.2"),
            new Port(1200),
            new Port(443),
            ProtocolType.FromRaw("6"),
            first,
            last,
            2048);

        var record = SessionDetectedAvroMapper.ToGenericRecord(session, occurred);

        Assert.Equal("SessionDetected", record["eventType"]);
        Assert.Equal("probe", record["source"]);
        Assert.Equal(1, record["schemaVersion"]);
        Assert.Equal(99, record["sessionId"]);
        Assert.Equal("10.0.0.1", record["sourceIp"]);
        Assert.Equal("10.0.0.2", record["destinationIp"]);
        Assert.Equal(1200, record["sourcePort"]);
        Assert.Equal(443, record["destinationPort"]);
        Assert.Equal("TCP", record["protocol"]);
        Assert.Equal("2025-03-15T10:30:00.0000000+00:00", record["firstSeenUtc"]);
        Assert.Equal("2025-03-15T10:31:05.0000000+00:00", record["lastSeenUtc"]);
        Assert.Equal(2048L, record["bytesObserved"]);
        Assert.Equal("2025-03-15T10:32:00.0000000+00:00", record["occurredAtUtc"]);
    }

    [Fact]
    public void ToGenericRecord_WithNullSessionId_SerializesUnionAsNull()
    {
        var t = DateTimeOffset.UtcNow;
        var session = Session.Create(
            null,
            new IpAddress("127.0.0.1"),
            new IpAddress("127.0.0.2"),
            new Port(1000),
            new Port(2000),
            ProtocolType.FromRaw("6"),
            t,
            t,
            1);

        var record = SessionDetectedAvroMapper.ToGenericRecord(session, t);

        Assert.Null(record["sessionId"]);
    }

    [Fact]
    public void SchemaInstance_ParsesEmbeddedContract()
    {
        var schema = SessionDetectedAvroMapper.SchemaInstance;
        Assert.Equal("SessionDetected", schema.Name);
        Assert.Equal("net.networkmonitoring.events", schema.Namespace);
    }
}
