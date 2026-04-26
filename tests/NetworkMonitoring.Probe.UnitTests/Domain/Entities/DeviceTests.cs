using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Domain.ValueObjects;

namespace NetworkMonitoring.Probe.UnitTests.Domain.Entities;

public sealed class DeviceTests
{
    [Fact]
    public void Create_WithObservedIps_StoresCollection()
    {
        var ip = new IpAddress("192.168.0.15");
        var device = Device.Create(
            null,
            new MacAddress("aa:bb:cc:dd:ee:ff"),
            ip,
            "host-a",
            [ip],
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            DiscoverySource.FromRaw("arp"));

        Assert.Null(device.Id);
        Assert.Single(device.ObservedIps);
        Assert.Equal("ARP", device.DiscoverySource.Value);
    }

    [Fact]
    public void ConsolidateDetection_WithNewTimestamp_UpdatesLastSeenAndHostname()
    {
        var firstSeen = new DateTimeOffset(2026, 4, 10, 10, 0, 0, TimeSpan.Zero);
        var secondSeen = firstSeen.AddMinutes(5);
        var device = Device.Create(
            null,
            new MacAddress("aa:bb:cc:dd:ee:ff"),
            new IpAddress("192.168.0.15"),
            "host-a",
            [new IpAddress("192.168.0.15")],
            firstSeen,
            firstSeen,
            DiscoverySource.FromRaw("arp"));

        device.ConsolidateDetection(
            new IpAddress("192.168.0.20"),
            "host-b",
            secondSeen,
            DiscoverySource.FromRaw("lldp"));

        Assert.Equal(firstSeen, device.FirstSeenUtc);
        Assert.Equal(secondSeen, device.LastSeenUtc);
        Assert.Equal("host-b", device.Hostname);
        Assert.Equal("LLDP", device.DiscoverySource.Value);
    }

    [Fact]
    public void ConsolidateDetection_WithEarlierTimestamp_UpdatesFirstSeenAndKeepsUniqueIps()
    {
        var firstSeen = new DateTimeOffset(2026, 4, 10, 10, 0, 0, TimeSpan.Zero);
        var earlierSeen = firstSeen.AddMinutes(-3);
        var originalIp = new IpAddress("192.168.0.15");
        var device = Device.Create(
            null,
            new MacAddress("aa:bb:cc:dd:ee:ff"),
            originalIp,
            "host-a",
            [originalIp],
            firstSeen,
            firstSeen,
            DiscoverySource.FromRaw("arp"));

        device.ConsolidateDetection(
            originalIp,
            null,
            earlierSeen,
            DiscoverySource.FromRaw("arp"));

        Assert.Equal(earlierSeen, device.FirstSeenUtc);
        Assert.Single(device.ObservedIps);
    }
}
