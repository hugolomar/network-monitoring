using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Domain.ValueObjects;
using NetworkMonitoring.Probe.Infrastructure.Publishing;

namespace NetworkMonitoring.Probe.UnitTests.Infrastructure.Publishing;

public sealed class DeviceKafkaPartitionKeyTests
{
    [Theory]
    [InlineData("aa-bb-cc-dd-ee-ff")]
    [InlineData("aa:bb:cc:dd:ee:ff")]
    [InlineData("AABBCCDDEEFF")]
    public void Build_UsesNormalizedMacAddress(string rawMac)
    {
        var device = CreateDevice(rawMac);

        Assert.Equal("AA:BB:CC:DD:EE:FF", DeviceKafkaPartitionKey.Build(device));
    }

    [Fact]
    public void BuildUtf8Bytes_RoundTripsWithUtf8()
    {
        var device = CreateDevice("01-23-45-67-89-ab");

        var bytes = DeviceKafkaPartitionKey.BuildUtf8Bytes(device);

        Assert.Equal(DeviceKafkaPartitionKey.Build(device), System.Text.Encoding.UTF8.GetString(bytes));
    }

    private static Device CreateDevice(string rawMac)
    {
        var t = DateTimeOffset.UtcNow;
        return Device.Create(
            null,
            new MacAddress(rawMac),
            new IpAddress("192.168.1.20"),
            null,
            [new IpAddress("192.168.1.20")],
            t,
            t,
            DiscoverySource.FromRaw("traffic"));
    }
}
