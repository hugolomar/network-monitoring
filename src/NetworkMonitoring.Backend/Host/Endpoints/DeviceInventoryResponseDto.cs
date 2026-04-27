using NetworkMonitoring.Backend.Application.Models;

namespace NetworkMonitoring.Backend.Host.Endpoints;

public sealed record DeviceInventoryResponseDto(IReadOnlyCollection<DeviceInventoryItem> Items);

public sealed record DeviceIntakeResponseDto(
    string Outcome,
    string? Reason,
    DeviceInventoryItem? Device);
