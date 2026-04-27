namespace NetworkMonitoring.Backend.Infrastructure.Persistence;

public sealed class DeviceInventoryRecord
{
    public int Id { get; set; }
    public string MacAddress { get; set; } = string.Empty;
    public string? PrimaryIp { get; set; }
    public string? Hostname { get; set; }
    public string ObservedIpsJson { get; set; } = "[]";
    public DateTimeOffset FirstSeenUtc { get; set; }
    public DateTimeOffset LastSeenUtc { get; set; }
    public string DiscoverySource { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
