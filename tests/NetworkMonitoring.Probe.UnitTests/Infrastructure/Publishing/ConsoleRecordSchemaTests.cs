using System.Text.Json;
using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Domain.ValueObjects;
using NetworkMonitoring.Probe.Infrastructure.Publishing;

namespace NetworkMonitoring.Probe.UnitTests.Infrastructure.Publishing;

/// <summary>
/// Test suite for ConsoleRecordSchema.
/// </summary>
public sealed class ConsoleRecordSchemaTests
{
    /// <summary>
    /// Verifies that serialize session contains expected envelope fields.
    /// </summary>
    [Fact]
    public void SerializeSession_ContainsExpectedEnvelopeFields()
    {
        var serializer = new ConsoleRecordSerializer();
        var now = new DateTimeOffset(2025, 3, 15, 10, 32, 0, TimeSpan.Zero);
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
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("SessionDetected", root.GetProperty("eventType").GetString());
        Assert.Equal(root.GetProperty("lastSeenUtc").GetString(), root.GetProperty("occurredAtUtc").GetString());
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.True(root.TryGetProperty("sessionId", out _));
    }

    /// <summary>
    /// Verifies that serialize device contains expected device detected fields.
    /// </summary>
    [Fact]
    public void SerializeDevice_ContainsExpectedDeviceDetectedFields()
    {
        var serializer = new ConsoleRecordSerializer();
        var now = new DateTimeOffset(2025, 4, 20, 9, 6, 0, TimeSpan.Zero);
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
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("DeviceDetected", root.GetProperty("eventType").GetString());
        Assert.Equal(root.GetProperty("lastSeenUtc").GetString(), root.GetProperty("occurredAtUtc").GetString());
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(42, root.GetProperty("deviceId").GetInt32());
        Assert.Equal("AA:BB:CC:DD:EE:FF", root.GetProperty("macAddress").GetString());
        Assert.Equal("192.168.1.10", root.GetProperty("observedIps")[0].GetString());
    }
}
