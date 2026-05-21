using NetworkMonitoring.Domain.Entities;

namespace NetworkMonitoring.Probe.Application.Ports;

/// <summary>
/// Defines a contract for publishing detected domain entities to external message systems (e.g., Kafka).
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a detected session to the messaging infrastructure.
    /// </summary>
    /// <param name="session">The session entity to publish.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous publication process.</returns>
    Task PublishSessionDetected(Session session, CancellationToken cancellationToken);

    /// <summary>
    /// Publishes a detected device to the messaging infrastructure.
    /// </summary>
    /// <param name="device">The device entity to publish.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous publication process.</returns>
    Task PublishDeviceDetected(Device device, CancellationToken cancellationToken);
}
