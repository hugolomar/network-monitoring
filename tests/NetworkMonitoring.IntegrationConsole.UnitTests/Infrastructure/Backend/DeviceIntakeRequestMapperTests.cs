using NetworkMonitoring.IntegrationConsole.Infrastructure.Backend;
using NetworkMonitoring.IntegrationConsole.UnitTests.Support;

namespace NetworkMonitoring.IntegrationConsole.UnitTests.Infrastructure.Backend;

public sealed class DeviceIntakeRequestMapperTests
{
    [Fact]
    public void Map_preserves_http_contract_body_fields()
    {
        var mapped = DeviceIntakeRequestMapper.Map(TestEvents.DeviceDetected());

        Assert.Equal("AA:BB:CC:DD:EE:FF", mapped.MacAddress);
        Assert.Equal("192.168.1.10", mapped.PrimaryIp);
        Assert.Equal("switch-01", mapped.Hostname);
        Assert.Equal(["192.168.1.10", "192.168.1.11"], mapped.ObservedIps);
        Assert.Equal("DeviceDetected", mapped.SourceEvent.EventType);
        Assert.Equal("probe", mapped.SourceEvent.Source);
    }
}
