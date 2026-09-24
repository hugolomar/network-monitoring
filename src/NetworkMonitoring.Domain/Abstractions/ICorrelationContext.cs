namespace NetworkMonitoring.Domain.Abstractions;

/// <summary>
/// Provides access to the current correlation and trace context.
/// </summary>
public interface ICorrelationContext
{
    /// <summary>
    /// Gets the correlation identifier associated with the current execution scope.
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// Gets the distributed trace identifier associated with the current execution scope.
    /// </summary>
    string? TraceId { get; }
}
