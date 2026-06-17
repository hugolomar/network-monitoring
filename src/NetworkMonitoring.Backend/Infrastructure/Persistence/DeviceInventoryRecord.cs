namespace NetworkMonitoring.Backend.Infrastructure.Persistence;

/// <summary>
/// Represents a device inventory record in the persistence store.
/// </summary>
public sealed class DeviceInventoryRecord
{
    /// <summary>
    /// Gets or sets the unique identifier for the device record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the MAC address of the device.
    /// </summary>
    public string MacAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary IP address of the device.
    /// </summary>
    public string? PrimaryIp { get; set; }

    /// <summary>
    /// Gets or sets the hostname of the device.
    /// </summary>
    public string? Hostname { get; set; }

    /// <summary>
    /// Gets or sets the JSON-serialized collection of observed IP addresses.
    /// </summary>
    public string ObservedIpsJson { get; set; } = "[]";

    /// <summary>
    /// Gets or sets the timestamp when the device was first seen in UTC.
    /// </summary>
    public DateTimeOffset FirstSeenUtc { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the device was last seen in UTC.
    /// </summary>
    public DateTimeOffset LastSeenUtc { get; set; }

    /// <summary>
    /// Gets or sets the source through which the device was discovered.
    /// </summary>
    public string DiscoverySource { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the record was created in UTC.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the record was last updated in UTC.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
