namespace NetworkMonitoring.IntegrationConsole.Application.Models;

/// <summary>
/// Represents an event consumed from the message broker, including metadata and optional payload.
/// </summary>
/// <param name="Key">The message key (usually MAC address).</param>
/// <param name="Event">The decoded domain event payload, if decoding was successful.</param>
/// <param name="Topic">The source topic.</param>
/// <param name="Partition">The source partition.</param>
/// <param name="Offset">The message offset.</param>
/// <param name="RejectionReason">Optional reason if the event was malformed or failed consumption.</param>
public sealed record ConsumedDeviceEvent(
    string? Key,
    DeviceDetectedEvent? Event,
    string Topic,
    int Partition,
    long Offset,
    string? RejectionReason = null)
{
    /// <summary>
    /// Gets a value indicating whether the event is malformed or could not be decoded.
    /// </summary>
    public bool IsMalformed => RejectionReason is not null || Event is null;
}
