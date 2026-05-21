using NetworkMonitoring.Backend.Application.Models;

namespace NetworkMonitoring.Backend.Host.Endpoints;

/// <summary>
/// Data transfer object representing the response for a list of devices in the inventory.
/// </summary>
/// <param name="Items">The collection of device inventory items.</param>
public sealed record DeviceInventoryResponseDto(IReadOnlyCollection<DeviceInventoryItem> Items);

/// <summary>
/// Data transfer object representing the response for a device intake operation.
/// </summary>
/// <param name="Outcome">The outcome of the intake operation (e.g., Created, Updated, Idempotent, Rejected).</param>
/// <param name="Reason">An optional message explaining the outcome or why a request was rejected.</param>
/// <param name="Device">The device inventory item as it exists after the operation, if applicable.</param>
public sealed record DeviceIntakeResponseDto(
    string Outcome,
    string? Reason,
    DeviceInventoryItem? Device);
