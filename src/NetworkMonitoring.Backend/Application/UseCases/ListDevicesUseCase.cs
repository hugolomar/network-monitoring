using NetworkMonitoring.Backend.Application.Models;
using NetworkMonitoring.Backend.Application.Ports;

namespace NetworkMonitoring.Backend.Application.UseCases;

/// <summary>
/// Use case for retrieving a list of all devices in the inventory.
/// </summary>
/// <param name="repository">The repository used to access device inventory data.</param>
public sealed class ListDevicesUseCase(IDeviceInventoryRepository repository)
{
    /// <summary>
    /// Executes the use case to list all devices.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only collection of <see cref="DeviceInventoryItem"/> representing the devices in the inventory, sorted by MAC address.</returns>
    public async Task<IReadOnlyCollection<DeviceInventoryItem>> Execute(CancellationToken cancellationToken)
    {
        var devices = await repository.List(cancellationToken);
        return devices
            .Select(AcceptDeviceIntakeUseCase.ToItem)
            .OrderBy(device => device.MacAddress, StringComparer.Ordinal)
            .ToArray();
    }
}
