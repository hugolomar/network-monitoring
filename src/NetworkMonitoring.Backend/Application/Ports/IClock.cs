namespace NetworkMonitoring.Backend.Application.Ports;

/// <summary>
/// Provides an abstraction for retrieving the current time.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current date and time in UTC.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
