using NetworkMonitoring.Domain.ValueObjects;

namespace NetworkMonitoring.Domain.Entities;

/// <summary>
/// Represents a network-connected device identified within the monitoring scope.
/// This entity tracks identity (MAC), addressing (IPs), and discovery timeline.
/// </summary>
public sealed class Device : Entity, IAggregateRoot
{
    private readonly HashSet<IpAddress> _observedIps = [];

    /// <summary>
    /// The unique physical hardware address of the device.
    /// </summary>
    public MacAddress MacAddress { get; private set; }

    /// <summary>
    /// The primary IP address currently associated with the device, if known.
    /// </summary>
    public IpAddress? PrimaryIp { get; private set; }

    /// <summary>
    /// The human-readable hostname assigned to the device.
    /// </summary>
    public string? Hostname { get; private set; }

    /// <summary>
    /// A collection of all IP addresses observed for this device during the monitoring session.
    /// </summary>
    public IReadOnlyCollection<IpAddress> ObservedIps => _observedIps;

    /// <summary>
    /// The timestamp of the first time this device was detected by the monitoring system.
    /// </summary>
    public DateTimeOffset FirstSeenUtc { get; private set; }

    /// <summary>
    /// The timestamp of the most recent activity detected for this device.
    /// </summary>
    public DateTimeOffset LastSeenUtc { get; private set; }

    /// <summary>
    /// The original source or mechanism through which the device was first discovered.
    /// </summary>
    public DiscoverySource DiscoverySource { get; private set; }

    private Device(
        int? id,
        MacAddress macAddress,
        IpAddress? primaryIp,
        string? hostname,
        IEnumerable<IpAddress>? observedIps,
        DateTimeOffset firstSeenUtc,
        DateTimeOffset lastSeenUtc,
        DiscoverySource discoverySource)
    {
        if (lastSeenUtc < firstSeenUtc)
        {
            throw new ArgumentException("LastSeenUtc must be greater than or equal to FirstSeenUtc.");
        }

        Id = id;
        MacAddress = macAddress;
        PrimaryIp = primaryIp;
        Hostname = string.IsNullOrWhiteSpace(hostname) ? null : hostname.Trim();
        FirstSeenUtc = firstSeenUtc;
        LastSeenUtc = lastSeenUtc;
        DiscoverySource = discoverySource;

        if (observedIps is not null)
        {
            foreach (var ip in observedIps)
            {
                _observedIps.Add(ip);
            }
        }
    }

    /// <summary>
    /// Creates a new instance of a <see cref="Device"/> entity.
    /// </summary>
    /// <param name="id">The persistence identifier. If less than or equal to 0, it is treated as null (unsaved).</param>
    /// <param name="macAddress">The physical MAC address (Required).</param>
    /// <param name="primaryIp">The main IP address associated with the device (Optional).</param>
    /// <param name="hostname">The device hostname (Optional).</param>
    /// <param name="observedIps">A list of IPs previously associated with this MAC (Optional).</param>
    /// <param name="firstSeenUtc">The initial discovery timestamp.</param>
    /// <param name="lastSeenUtc">The latest discovery timestamp. Must be greater than or equal to <paramref name="firstSeenUtc"/>.</param>
    /// <param name="discoverySource">The source that triggered the creation.</param>
    /// <returns>A fully initialized <see cref="Device"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="lastSeenUtc"/> is before <paramref name="firstSeenUtc"/>.</exception>
    public static Device Create(
        int? id,
        MacAddress macAddress,
        IpAddress? primaryIp,
        string? hostname,
        IEnumerable<IpAddress>? observedIps,
        DateTimeOffset firstSeenUtc,
        DateTimeOffset lastSeenUtc,
        DiscoverySource discoverySource)
    {
        return new Device(
            ResolvePersistentId(id),
            macAddress,
            primaryIp,
            hostname,
            observedIps,
            firstSeenUtc,
            lastSeenUtc,
            discoverySource);
    }

    private static int? ResolvePersistentId(int? id) => id is > 0 ? id : null;

    /// <summary>
    /// Updates the device's observation history and metadata based on a new detection event.
    /// </summary>
    /// <param name="observedIp">The IP address where the device was just seen (Optional).</param>
    /// <param name="hostname">The hostname reported in the new detection (Optional).</param>
    /// <param name="observedAtUtc">The timestamp of the new detection.</param>
    /// <param name="discoverySource">The source that provided the new detection information.</param>
    public void ConsolidateDetection(
        IpAddress? observedIp,
        string? hostname,
        DateTimeOffset observedAtUtc,
        DiscoverySource discoverySource)
    {
        if (observedAtUtc < FirstSeenUtc)
        {
            FirstSeenUtc = observedAtUtc;
        }

        if (observedAtUtc > LastSeenUtc)
        {
            LastSeenUtc = observedAtUtc;
        }

        if (observedIp is not null)
        {
            _observedIps.Add(observedIp);
            PrimaryIp ??= observedIp;
        }

        if (!string.IsNullOrWhiteSpace(hostname))
        {
            Hostname = hostname.Trim();
        }

        DiscoverySource = discoverySource;
    }
}
