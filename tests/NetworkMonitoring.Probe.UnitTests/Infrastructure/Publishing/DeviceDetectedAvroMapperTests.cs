using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Domain.ValueObjects;
using NetworkMonitoring.Probe.Infrastructure.Publishing;

namespace NetworkMonitoring.Probe.UnitTests.Infrastructure.Publishing;

public sealed class DeviceDetectedAvroMapperTests
{
    [Fact]
    public void ToGenericRecord_MapsAllContractFields()
    {
        var first = new DateTimeOffset(2025, 4, 20, 9, 0, 0, TimeSpan.Zero);
        var last = new DateTimeOffset(2025, 4, 20, 9, 5, 0, TimeSpan.Zero);
        var occurred = new DateTimeOffset(2025, 4, 20, 9, 6, 0, TimeSpan.Zero);

        var device = Device.Create(
            42,
            new MacAddress("aa-bb-cc-dd-ee-ff"),
            new IpAddress("192.168.10.20"),
            " switch-01 ",
            [new IpAddress("192.168.10.21"), new IpAddress("192.168.10.20")],
            first,
            last,
            DiscoverySource.FromRaw("arp"));

        var record = DeviceDetectedAvroMapper.ToGenericRecord(device, occurred);

        Assert.Equal("DeviceDetected", record["eventType"]);
        Assert.Equal("probe", record["source"]);
        Assert.Equal(1, record["schemaVersion"]);
        Assert.Equal(42, record["deviceId"]);
        Assert.Equal("AA:BB:CC:DD:EE:FF", record["macAddress"]);
        Assert.Equal("192.168.10.20", record["primaryIp"]);
        Assert.Equal("switch-01", record["hostname"]);
        Assert.Equal(new[] { "192.168.10.20", "192.168.10.21" }, (string[])record["observedIps"]);
        Assert.Equal("2025-04-20T09:00:00.0000000+00:00", record["firstSeenUtc"]);
        Assert.Equal("2025-04-20T09:05:00.0000000+00:00", record["lastSeenUtc"]);
        Assert.Equal("ARP", record["discoverySource"]);
        Assert.Equal("2025-04-20T09:06:00.0000000+00:00", record["occurredAtUtc"]);
    }

    [Fact]
    public void ToGenericRecord_WithOptionalValuesAbsent_SerializesUnionsAsNull()
    {
        var t = DateTimeOffset.UtcNow;
        var device = Device.Create(
            null,
            new MacAddress("AABBCCDDEEFF"),
            null,
            null,
            null,
            t,
            t,
            DiscoverySource.FromRaw(null));

        var record = DeviceDetectedAvroMapper.ToGenericRecord(device, t);

        Assert.Null(record["deviceId"]);
        Assert.Null(record["primaryIp"]);
        Assert.Null(record["hostname"]);
        Assert.Empty((string[])record["observedIps"]);
    }

    [Fact]
    public void SchemaInstance_ParsesEmbeddedContract()
    {
        var schema = DeviceDetectedAvroMapper.SchemaInstance;
        Assert.Equal("DeviceDetected", schema.Name);
        Assert.Equal("net.networkmonitoring.events", schema.Namespace);
    }
}
