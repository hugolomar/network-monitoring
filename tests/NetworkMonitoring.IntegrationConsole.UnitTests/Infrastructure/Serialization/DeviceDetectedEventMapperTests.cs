using Avro;
using Avro.Generic;
using NetworkMonitoring.IntegrationConsole.Infrastructure.Serialization;

namespace NetworkMonitoring.IntegrationConsole.UnitTests.Infrastructure.Serialization;

public sealed class DeviceDetectedEventMapperTests
{
    [Fact]
    public void FromGenericRecord_maps_DeviceDetected_contract_fields()
    {
        var schemaPath = Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../specs/003-device-discovery/contracts/device-detected-value.avsc");
        var schema = (RecordSchema)Schema.Parse(File.ReadAllText(schemaPath));
        var record = new GenericRecord(schema);
        record.Add("eventType", "DeviceDetected");
        record.Add("occurredAtUtc", "2026-04-27T12:05:01.0000000+00:00");
        record.Add("source", "probe");
        record.Add("schemaVersion", 1);
        record.Add("deviceId", null);
        record.Add("macAddress", "AA:BB:CC:DD:EE:FF");
        record.Add("primaryIp", "192.168.1.10");
        record.Add("hostname", "switch-01");
        record.Add("observedIps", new[] { "192.168.1.10", "192.168.1.11" });
        record.Add("firstSeenUtc", "2026-04-27T12:00:00.0000000+00:00");
        record.Add("lastSeenUtc", "2026-04-27T12:05:00.0000000+00:00");
        record.Add("discoverySource", "TRAFFIC");

        var mapped = DeviceDetectedEventMapper.FromGenericRecord(record);

        Assert.Equal("DeviceDetected", mapped.EventType);
        Assert.Equal("AA:BB:CC:DD:EE:FF", mapped.MacAddress);
        Assert.Equal(["192.168.1.10", "192.168.1.11"], mapped.ObservedIps);
        Assert.Equal("TRAFFIC", mapped.DiscoverySource);
    }
}
