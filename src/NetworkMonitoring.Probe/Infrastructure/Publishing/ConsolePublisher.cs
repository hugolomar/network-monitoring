using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Probe.Application.Ports;

namespace NetworkMonitoring.Probe.Infrastructure.Publishing;

/// <summary>
/// Implementation of <see cref="IMessagePublisher"/> that outputs detection events to the standard console output.
/// Primarily used for development, debugging, and terminal-based monitoring.
/// </summary>
public sealed class ConsolePublisher(ConsoleRecordSerializer serializer) : IMessagePublisher
{
    /// <summary>
    /// Serializes and writes a session detection event to the console.
    /// </summary>
    /// <param name="session">The session entity to output.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A completed task.</returns>
    public Task PublishSessionDetected(Session session, CancellationToken cancellationToken)
    {
        Console.WriteLine(serializer.SerializeSession(session));
        return Task.CompletedTask;
    }

    /// <summary>
    /// Serializes and writes a device detection event to the console.
    /// </summary>
    /// <param name="device">The device entity to output.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A completed task.</returns>
    public Task PublishDeviceDetected(Device device, CancellationToken cancellationToken)
    {
        Console.WriteLine(serializer.SerializeDevice(device));
        return Task.CompletedTask;
    }
}
