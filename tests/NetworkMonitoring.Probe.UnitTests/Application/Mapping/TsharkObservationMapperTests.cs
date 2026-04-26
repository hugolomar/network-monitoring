using NetworkMonitoring.Probe.Infrastructure.Traffic;

namespace NetworkMonitoring.Probe.UnitTests.Application.Mapping;

public sealed class TsharkObservationMapperTests
{
    [Fact]
    public void TryMap_WithValidLine_ReturnsObservation()
    {
        var mapper = new TsharkObservationMapper();
        var line = "10.0.0.1\t10.0.0.2\t1234\t443\t6\t1712164800.123456\t120\tAA:BB:CC:DD:EE:FF\t11:22:33:44:55:66\thost-a";

        var ok = mapper.TryMap(line, out var observation);

        Assert.True(ok);
        Assert.NotNull(observation);
        Assert.Equal("10.0.0.1", observation!.SourceIp);
        Assert.Equal(1234, observation.SourcePort);
        Assert.Equal(120, observation.BytesObserved);
        Assert.Equal("TRAFFIC", observation.DiscoverySource);
    }

    [Fact]
    public void TryMap_WithMalformedLine_ReturnsFalse()
    {
        var mapper = new TsharkObservationMapper();
        var ok = mapper.TryMap("bad-line", out _);
        Assert.False(ok);
    }
}
