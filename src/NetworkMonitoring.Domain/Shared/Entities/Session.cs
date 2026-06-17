using NetworkMonitoring.Domain.ValueObjects;

namespace NetworkMonitoring.Domain.Entities;

/// <summary>
/// Represents a network communication session between two endpoints.
/// This entity captures the traffic metadata (IPs, ports, protocol) and volume observed.
/// </summary>
public sealed class Session : Entity
{
    /// <summary>
    /// The IP address of the originator of the session.
    /// </summary>
    public IpAddress SourceIp { get; private set; }

    /// <summary>
    /// The IP address of the target of the session.
    /// </summary>
    public IpAddress DestinationIp { get; private set; }

    /// <summary>
    /// The transport layer source port (Optional, depending on protocol).
    /// </summary>
    public Port? SourcePort { get; private set; }

    /// <summary>
    /// The transport layer destination port (Optional, depending on protocol).
    /// </summary>
    public Port? DestinationPort { get; private set; }

    /// <summary>
    /// The transport protocol used in the session (e.g., TCP, UDP).
    /// </summary>
    public ProtocolType Protocol { get; private set; }

    /// <summary>
    /// The timestamp of the first packet observed in this session.
    /// </summary>
    public DateTimeOffset FirstSeenUtc { get; private set; }

    /// <summary>
    /// The timestamp of the most recent packet observed in this session.
    /// </summary>
    public DateTimeOffset LastSeenUtc { get; private set; }

    /// <summary>
    /// The total volume of traffic (in bytes) observed for this session.
    /// </summary>
    public long BytesObserved { get; private set; }

    private Session(
        int? id,
        IpAddress sourceIp,
        IpAddress destinationIp,
        Port? sourcePort,
        Port? destinationPort,
        ProtocolType protocol,
        DateTimeOffset firstSeenUtc,
        DateTimeOffset lastSeenUtc,
        long bytesObserved)
    {
        if (lastSeenUtc < firstSeenUtc)
        {
            throw new ArgumentException("LastSeenUtc must be greater than or equal to FirstSeenUtc.");
        }

        if (bytesObserved < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bytesObserved), "Bytes observed cannot be negative.");
        }

        Id = id;
        SourceIp = sourceIp;
        DestinationIp = destinationIp;
        SourcePort = sourcePort;
        DestinationPort = destinationPort;
        Protocol = protocol;
        FirstSeenUtc = firstSeenUtc;
        LastSeenUtc = lastSeenUtc;
        BytesObserved = bytesObserved;
    }

    /// <summary>
    /// Creates a new instance of a <see cref="Session"/> entity.
    /// </summary>
    /// <param name="id">The persistence identifier. If less than or equal to 0, it is treated as null (unsaved).</param>
    /// <param name="sourceIp">The originator's IP address (Required).</param>
    /// <param name="destinationIp">The target's IP address (Required).</param>
    /// <param name="sourcePort">The originator's transport port (Optional).</param>
    /// <param name="destinationPort">The target's transport port (Optional).</param>
    /// <param name="protocol">The transport protocol (Required).</param>
    /// <param name="firstSeenUtc">The start timestamp of the session.</param>
    /// <param name="lastSeenUtc">The latest activity timestamp. Must be greater than or equal to <paramref name="firstSeenUtc"/>.</param>
    /// <param name="bytesObserved">The total bytes transferred. Must be non-negative.</param>
    /// <returns>A fully initialized <see cref="Session"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="lastSeenUtc"/> is before <paramref name="firstSeenUtc"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="bytesObserved"/> is negative.</exception>
    public static Session Create(
        int? id,
        IpAddress sourceIp,
        IpAddress destinationIp,
        Port? sourcePort,
        Port? destinationPort,
        ProtocolType protocol,
        DateTimeOffset firstSeenUtc,
        DateTimeOffset lastSeenUtc,
        long bytesObserved)
    {
        return new Session(
            ResolvePersistentId(id),
            sourceIp,
            destinationIp,
            sourcePort,
            destinationPort,
            protocol,
            firstSeenUtc,
            lastSeenUtc,
            bytesObserved);
    }

    private static int? ResolvePersistentId(int? id) => id is > 0 ? id : null;
}
