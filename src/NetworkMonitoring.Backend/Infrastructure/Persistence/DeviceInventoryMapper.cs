using System.Text.Json;
using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Domain.ValueObjects;

namespace NetworkMonitoring.Backend.Infrastructure.Persistence;

/// <summary>
/// Provides mapping functionality between domain <see cref="Device"/> entities and persistence <see cref="DeviceInventoryRecord"/> records.
/// </summary>
public static class DeviceInventoryMapper
{
    /// <summary>
    /// Maps a <see cref="DeviceInventoryRecord"/> to a domain <see cref="Device"/> entity.
    /// </summary>
    /// <param name="record">The database record to map.</param>
    /// <returns>A <see cref="Device"/> entity populated with data from the record.</returns>
    public static Device ToDomain(DeviceInventoryRecord record)
    {
        var observedIps = JsonSerializer.Deserialize<string[]>(record.ObservedIpsJson) ?? [];
        return Device.Create(
            record.Id,
            new MacAddress(record.MacAddress),
            record.PrimaryIp is null ? null : new IpAddress(record.PrimaryIp),
            record.Hostname,
            observedIps.Select(ip => new IpAddress(ip)),
            record.FirstSeenUtc,
            record.LastSeenUtc,
            DiscoverySource.FromRaw(record.DiscoverySource));
    }

    /// <summary>
    /// Maps a domain <see cref="Device"/> entity to a new <see cref="DeviceInventoryRecord"/>.
    /// </summary>
    /// <param name="device">The domain entity to map.</param>
    /// <param name="now">The current timestamp to use for creation and update dates.</param>
    /// <returns>A new <see cref="DeviceInventoryRecord"/>.</returns>
    public static DeviceInventoryRecord ToRecord(Device device, DateTimeOffset now)
    {
        return new DeviceInventoryRecord
        {
            Id = device.Id ?? 0,
            MacAddress = device.MacAddress.Value,
            PrimaryIp = device.PrimaryIp?.Value,
            Hostname = device.Hostname,
            ObservedIpsJson = SerializeObservedIps(device),
            FirstSeenUtc = device.FirstSeenUtc,
            LastSeenUtc = device.LastSeenUtc,
            DiscoverySource = device.DiscoverySource.Value,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    /// <summary>
    /// Updates an existing <see cref="DeviceInventoryRecord"/> with data from a domain <see cref="Device"/> entity.
    /// </summary>
    /// <param name="record">The database record to update.</param>
    /// <param name="device">The domain entity containing the new data.</param>
    /// <param name="now">The current timestamp to use for the update date.</param>
    public static void UpdateRecord(DeviceInventoryRecord record, Device device, DateTimeOffset now)
    {
        record.PrimaryIp = device.PrimaryIp?.Value;
        record.Hostname = device.Hostname;
        record.ObservedIpsJson = SerializeObservedIps(device);
        record.FirstSeenUtc = device.FirstSeenUtc;
        record.LastSeenUtc = device.LastSeenUtc;
        record.DiscoverySource = device.DiscoverySource.Value;
        record.UpdatedAtUtc = now;
    }

    // Serializes the collection of observed IP addresses to a JSON array, sorted for consistency.
    private static string SerializeObservedIps(Device device)
    {
        return JsonSerializer.Serialize(device.ObservedIps.Select(ip => ip.Value).Order().ToArray());
    }
}
