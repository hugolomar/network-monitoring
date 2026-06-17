using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Probe.Application.Ports;

namespace NetworkMonitoring.Probe.Infrastructure.Publishing;

/// <summary>
/// An implementation of <see cref="IMessagePublisher"/> that broadcasts messages to a collection of other publishers.
/// Allows simultaneous publication to multiple sinks (e.g., Console and Kafka).
/// </summary>
/// <param name="publishers">The list of underlying publishers to delegate to.</param>
public sealed class CompositeMessagePublisher(IReadOnlyList<IMessagePublisher> publishers) : IMessagePublisher
{
    /// <summary>
    /// Broadcasts a session detection event to all registered publishers.
    /// </summary>
    /// <param name="session">The session to publish.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the broadcast operation.</returns>
    public async Task PublishSessionDetected(Session session, CancellationToken cancellationToken)
    {
        foreach (var publisher in publishers)
        {
            await publisher.PublishSessionDetected(session, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Broadcasts a device detection event to all registered publishers.
    /// </summary>
    /// <param name="device">The device to publish.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the broadcast operation.</returns>
    public async Task PublishDeviceDetected(Device device, CancellationToken cancellationToken)
    {
        foreach (var publisher in publishers)
        {
            await publisher.PublishDeviceDetected(device, cancellationToken).ConfigureAwait(false);
        }
    }
}
