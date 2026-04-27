using NetworkMonitoring.Backend.Application.Ports;
using NetworkMonitoring.Domain.Entities;

namespace NetworkMonitoring.Backend.UnitTests.Support;

internal sealed class InMemoryDeviceInventoryRepository : IDeviceInventoryRepository, IInventoryUnitOfWork
{
    private readonly Dictionary<string, Device> _devices = new(StringComparer.Ordinal);

    public bool ThrowOnSave { get; set; }

    public Task<Device?> GetByMacAddress(string normalizedMacAddress, CancellationToken cancellationToken)
    {
        _devices.TryGetValue(normalizedMacAddress, out var device);
        return Task.FromResult(device);
    }

    public Task Add(Device device, CancellationToken cancellationToken)
    {
        var persisted = Device.Create(
            1,
            device.MacAddress,
            device.PrimaryIp,
            device.Hostname,
            device.ObservedIps,
            device.FirstSeenUtc,
            device.LastSeenUtc,
            device.DiscoverySource);
        _devices[device.MacAddress.Value] = persisted;
        return Task.CompletedTask;
    }

    public Task Update(Device device, CancellationToken cancellationToken)
    {
        _devices[device.MacAddress.Value] = device;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<Device>> List(CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyCollection<Device>>(_devices.Values.ToArray());
    }

    public Task SaveChanges(CancellationToken cancellationToken)
    {
        if (ThrowOnSave)
        {
            throw new InvalidOperationException("Persistence unavailable");
        }

        return Task.CompletedTask;
    }
}
