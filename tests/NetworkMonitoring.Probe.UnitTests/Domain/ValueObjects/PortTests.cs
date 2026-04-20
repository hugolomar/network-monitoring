using NetworkMonitoring.Domain.ValueObjects;

namespace NetworkMonitoring.Probe.UnitTests.Domain.ValueObjects;

public sealed class PortTests
{
    [Fact]
    public void Constructor_WithValidPort_CreatesValueObject()
    {
        var value = new Port(443);
        Assert.Equal(443, value.Value);
    }

    [Fact]
    public void Constructor_WithOutOfRangePort_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Port(70000));
    }
}
