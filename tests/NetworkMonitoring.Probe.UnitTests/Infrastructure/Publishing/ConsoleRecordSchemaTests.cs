using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Domain.ValueObjects;
using NetworkMonitoring.Probe.Infrastructure.Publishing;

namespace NetworkMonitoring.Probe.UnitTests.Infrastructure.Publishing;

public sealed class ConsoleRecordSchemaTests
{
    [Fact]
    public void SerializeSession_ContainsExpectedEnvelopeFields()
    {
        var serializer = new ConsoleRecordSerializer();
        var now = DateTimeOffset.UtcNow;
        var session = Session.Create(
            1,
            new IpAddress("10.1.1.1"),
            new IpAddress("10.1.1.2"),
            new Port(5555),
            new Port(443),
            ProtocolType.FromRaw("TCP"),
            now,
            now,
            10);

        var json = serializer.SerializeSession(session);

        Assert.Contains("\"eventType\":\"SessionDetected\"", json);
        Assert.Contains("\"schemaVersion\":1", json);
        Assert.Contains("\"sessionId\"", json);
    }

    [Fact]
    public void SerializeDevice_ContainsExpectedDeviceDetectedFields()
    {
        var serializer = new ConsoleRecordSerializer();
        var now = DateTimeOffset.UtcNow;
        var ip = new IpAddress("192.168.1.10");
        var device = Device.Create(
            42,
            new MacAddress("aa:bb:cc:dd:ee:ff"),
            ip,
            "edge-switch",
            [ip],
            now,
            now,
            DiscoverySource.FromRaw("arp"));

        var json = serializer.SerializeDevice(device);

        Assert.Contains("\"eventType\":\"DeviceDetected\"", json);
        Assert.Contains("\"schemaVersion\":1", json);
        Assert.Contains("\"deviceId\":42", json);
        Assert.Contains("\"macAddress\":\"AA:BB:CC:DD:EE:FF\"", json);
        Assert.Contains("\"observedIps\":[\"192.168.1.10\"]", json);
    }
}
