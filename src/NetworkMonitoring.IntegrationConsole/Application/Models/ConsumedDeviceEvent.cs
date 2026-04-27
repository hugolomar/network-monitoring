namespace NetworkMonitoring.IntegrationConsole.Application.Models;

public sealed record ConsumedDeviceEvent(
    string? Key,
    DeviceDetectedEvent? Event,
    string Topic,
    int Partition,
    long Offset,
    string? RejectionReason = null)
{
    public bool IsMalformed => RejectionReason is not null || Event is null;
}
