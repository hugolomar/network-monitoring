using NetworkMonitoring.Domain.ValueObjects;

namespace NetworkMonitoring.Domain.Entities;

public sealed class Device : Entity, IAggregateRoot
{
    private readonly HashSet<IpAddress> _observedIps = [];

    public MacAddress MacAddress { get; private set; }
    public IpAddress? PrimaryIp { get; private set; }
    public string? Hostname { get; private set; }
    public IReadOnlyCollection<IpAddress> ObservedIps => _observedIps;
    public DateTimeOffset FirstSeenUtc { get; private set; }
    public DateTimeOffset LastSeenUtc { get; private set; }
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
