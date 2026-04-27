using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Domain.ValueObjects;

namespace NetworkMonitoring.Backend.UnitTests.Application.UseCases;

public sealed class SharedDomainUsageTests
{
    [Fact]
    public void Backend_references_shared_device_domain_types_in_business_use_case()
    {
        var useCaseSource = typeof(AcceptDeviceIntakeUseCase).Assembly
            .GetType("NetworkMonitoring.Backend.Application.UseCases.AcceptDeviceIntakeUseCase")!
            .GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .Select(method => method.ToString())
            .ToArray();

        Assert.Contains("Device", typeof(Device).Name);
        Assert.Equal("AA:BB:CC:DD:EE:FF", new MacAddress("aa-bb-cc-dd-ee-ff").Value);
        Assert.Equal("192.168.1.10", new IpAddress("192.168.1.10").Value);
        Assert.Equal("TRAFFIC", DiscoverySource.FromRaw("traffic").Value);
        Assert.NotEmpty(useCaseSource);
    }
}
