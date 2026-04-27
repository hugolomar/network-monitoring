using NetworkMonitoring.IntegrationConsole.Application.Models;

namespace NetworkMonitoring.IntegrationConsole.Application.Ports;

public interface IDeviceIntakeClient
{
    Task<IngestionOutcome> Send(DeviceDetectedEvent detectedEvent, CancellationToken cancellationToken);
}
