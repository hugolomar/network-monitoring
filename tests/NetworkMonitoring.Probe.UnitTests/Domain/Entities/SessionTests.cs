using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Domain.ValueObjects;

namespace NetworkMonitoring.Probe.UnitTests.Domain.Entities;

public sealed class SessionTests
{
    [Fact]
    public void Create_WithValidValues_ReturnsSession()
    {
        var session = Session.Create(
            null,
            new IpAddress("10.0.0.1"),
            new IpAddress("10.0.0.2"),
            new Port(1200),
            new Port(443),
            ProtocolType.FromRaw("6"),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            128);

        Assert.Null(session.Id);
        Assert.Equal("TCP", session.Protocol.Value);
    }
}
