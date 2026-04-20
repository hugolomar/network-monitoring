using NetworkMonitoring.Domain.ValueObjects;

namespace NetworkMonitoring.Probe.UnitTests.Domain.ValueObjects;

public sealed class MacAddressTests
{
    [Fact]
    public void Constructor_WithValidMac_NormalizesFormat()
    {
        var value = new MacAddress("aa-bb-cc-dd-ee-ff");
        Assert.Equal("AA:BB:CC:DD:EE:FF", value.Value);
    }

    [Fact]
    public void Constructor_WithInvalidMac_Throws()
    {
        Assert.Throws<ArgumentException>(() => new MacAddress("aa-bb"));
    }
}
