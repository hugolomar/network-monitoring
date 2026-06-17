using NetworkMonitoring.Backend.Application.Ports;

namespace NetworkMonitoring.Backend.Infrastructure;

/// <summary>
/// System clock implementation providing the current UTC time.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
