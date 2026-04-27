using System.Text.Json;
using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Domain.ValueObjects;

namespace NetworkMonitoring.Backend.Infrastructure.Persistence;

public static class DeviceInventoryMapper
{
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

    private static string SerializeObservedIps(Device device)
    {
        return JsonSerializer.Serialize(device.ObservedIps.Select(ip => ip.Value).Order().ToArray());
    }
}
