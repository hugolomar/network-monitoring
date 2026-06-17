using NetworkMonitoring.Probe.Application.Models;

namespace NetworkMonitoring.Probe.Application.Ports;

/// <summary>
/// Defines a contract for components that provide a stream of raw network traffic observations.
/// </summary>
public interface ITrafficProvider
{
    /// <summary>
    /// Reads traffic observations from an underlying source (e.g., live capture or file).
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>An asynchronous stream of <see cref="TrafficObservation"/> objects.</returns>
    IAsyncEnumerable<TrafficObservation> ReadObservations(CancellationToken cancellationToken);
}
