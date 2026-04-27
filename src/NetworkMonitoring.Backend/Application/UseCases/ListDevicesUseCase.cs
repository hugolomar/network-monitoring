using NetworkMonitoring.Backend.Application.Models;
using NetworkMonitoring.Backend.Application.Ports;

namespace NetworkMonitoring.Backend.Application.UseCases;

public sealed class ListDevicesUseCase(IDeviceInventoryRepository repository)
{
    public async Task<IReadOnlyCollection<DeviceInventoryItem>> Execute(CancellationToken cancellationToken)
    {
        var devices = await repository.List(cancellationToken);
        return devices
            .Select(AcceptDeviceIntakeUseCase.ToItem)
            .OrderBy(device => device.MacAddress, StringComparer.Ordinal)
            .ToArray();
    }
}
