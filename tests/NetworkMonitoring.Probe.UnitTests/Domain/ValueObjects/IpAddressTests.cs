using NetworkMonitoring.Domain.ValueObjects;

namespace NetworkMonitoring.Probe.UnitTests.Domain.ValueObjects;

/// <summary>
/// Test suite for IpAddress.
/// </summary>
public sealed class IpAddressTests
{
    /// <summary>
    /// Verifies that constructor with valid ip creates value object.
    /// </summary>
    [Fact]
    public void Constructor_WithValidIp_CreatesValueObject()
    {
        var value = new IpAddress("192.168.1.10");
        Assert.Equal("192.168.1.10", value.Value);
    }

    /// <summary>
    /// Verifies that constructor with invalid ip throws.
    /// </summary>
    [Fact]
    public void Constructor_WithInvalidIp_Throws()
    {
        Assert.Throws<ArgumentException>(() => new IpAddress("not-an-ip"));
    }
}
