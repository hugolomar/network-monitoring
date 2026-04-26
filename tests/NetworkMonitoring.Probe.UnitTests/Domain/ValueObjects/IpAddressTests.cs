using NetworkMonitoring.Domain.ValueObjects;

namespace NetworkMonitoring.Probe.UnitTests.Domain.ValueObjects;

public sealed class IpAddressTests
{
    [Fact]
    public void Constructor_WithValidIp_CreatesValueObject()
    {
        var value = new IpAddress("192.168.1.10");
        Assert.Equal("192.168.1.10", value.Value);
    }

    [Fact]
    public void Constructor_WithInvalidIp_Throws()
    {
        Assert.Throws<ArgumentException>(() => new IpAddress("not-an-ip"));
    }
}
