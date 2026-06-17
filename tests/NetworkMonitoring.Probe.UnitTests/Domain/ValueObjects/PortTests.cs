using NetworkMonitoring.Domain.ValueObjects;

namespace NetworkMonitoring.Probe.UnitTests.Domain.ValueObjects;

/// <summary>
/// Test suite for Port.
/// </summary>
public sealed class PortTests
{
    /// <summary>
    /// Verifies that constructor with valid port creates value object.
    /// </summary>
    [Fact]
    public void Constructor_WithValidPort_CreatesValueObject()
    {
        var value = new Port(443);
        Assert.Equal(443, value.Value);
    }

    /// <summary>
    /// Verifies that constructor with out of range port throws.
    /// </summary>
    [Fact]
    public void Constructor_WithOutOfRangePort_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Port(70000));
    }
}
