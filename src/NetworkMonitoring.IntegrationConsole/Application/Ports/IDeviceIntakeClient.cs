using NetworkMonitoring.IntegrationConsole.Application.Models;

namespace NetworkMonitoring.IntegrationConsole.Application.Ports;

/// <summary>
/// Port for sending device detection events to the backend system.
/// </summary>
public interface IDeviceIntakeClient
{
    /// <summary>
    /// Sends a detected device event to the backend.
    /// </summary>
    /// <param name="detectedEvent">The event to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The outcome of the ingestion attempt.</returns>
    Task<IngestionOutcome> Send(DeviceDetectedEvent detectedEvent, CancellationToken cancellationToken);
}
