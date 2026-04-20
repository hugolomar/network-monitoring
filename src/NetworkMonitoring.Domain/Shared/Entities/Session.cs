using NetworkMonitoring.Domain.ValueObjects;

namespace NetworkMonitoring.Domain.Entities;

public sealed class Session : Entity
{
    public IpAddress SourceIp { get; private set; }
    public IpAddress DestinationIp { get; private set; }
    public Port? SourcePort { get; private set; }
    public Port? DestinationPort { get; private set; }
    public ProtocolType Protocol { get; private set; }
    public DateTimeOffset FirstSeenUtc { get; private set; }
    public DateTimeOffset LastSeenUtc { get; private set; }
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
