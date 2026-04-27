using NetworkMonitoring.Domain.Entities;

namespace NetworkMonitoring.Backend.Application.Ports;

public interface IDeviceInventoryRepository
{
    Task<Device?> GetByMacAddress(string normalizedMacAddress, CancellationToken cancellationToken);

    Task Add(Device device, CancellationToken cancellationToken);

    Task Update(Device device, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Device>> List(CancellationToken cancellationToken);
}
