using NetworkMonitoring.Probe.Application.Models;

namespace NetworkMonitoring.Probe.Application.Ports;

public interface ITrafficProvider
{
    IAsyncEnumerable<TrafficObservation> ReadObservations(CancellationToken cancellationToken);
}
