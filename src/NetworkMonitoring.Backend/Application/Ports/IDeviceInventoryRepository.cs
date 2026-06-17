using NetworkMonitoring.Domain.Entities;

namespace NetworkMonitoring.Backend.Application.Ports;

/// <summary>
/// Defines the contract for a repository managing device inventory.
/// </summary>
public interface IDeviceInventoryRepository
{
    /// <summary>
    /// Retrieves a device by its MAC address.
    /// </summary>
    /// <param name="normalizedMacAddress">The normalized MAC address of the device.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the device if found; otherwise, <see langword="null"/>.</returns>
    Task<Device?> GetByMacAddress(string normalizedMacAddress, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new device to the inventory.
    /// </summary>
    /// <param name="device">The device to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Add(Device device, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing device in the inventory.
    /// </summary>
    /// <param name="device">The device to update.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Update(Device device, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all devices in the inventory.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of all devices.</returns>
    Task<IReadOnlyCollection<Device>> List(CancellationToken cancellationToken);
}
