using NetworkMonitoring.Domain.ValueObjects;

namespace NetworkMonitoring.Probe.UnitTests.Domain.ValueObjects;

/// <summary>
/// Test suite for MacAddress.
/// </summary>
public sealed class MacAddressTests
{
    /// <summary>
    /// Verifies that constructor with valid mac normalizes format.
    /// </summary>
    [Fact]
    public void Constructor_WithValidMac_NormalizesFormat()
    {
        var value = new MacAddress("aa-bb-cc-dd-ee-ff");
        Assert.Equal("AA:BB:CC:DD:EE:FF", value.Value);
    }

    /// <summary>
    /// Verifies that constructor with invalid mac throws.
    /// </summary>
    [Fact]
    public void Constructor_WithInvalidMac_Throws()
    {
        Assert.Throws<ArgumentException>(() => new MacAddress("aa-bb"));
    }
}
