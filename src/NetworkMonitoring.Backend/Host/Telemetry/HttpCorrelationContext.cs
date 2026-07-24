using System.Diagnostics;
using NetworkMonitoring.Domain.Abstractions;

namespace NetworkMonitoring.Backend.Host.Telemetry;

/// <summary>
/// HTTP-backed correlation context projection.
/// </summary>
internal sealed class HttpCorrelationContext(IHttpContextAccessor httpContextAccessor) : ICorrelationContext
{
    private const string CorrelationItemKey = "X-Correlation-ID";

    /// <inheritdoc />
    public string CorrelationId
    {
        get
        {
            var context = httpContextAccessor.HttpContext;
            if (context is null)
            {
                return Activity.Current?.TraceId.ToString() ?? string.Empty;
            }

            if (context.Items.TryGetValue(CorrelationItemKey, out var value) && value is string correlation && !string.IsNullOrWhiteSpace(correlation))
            {
                return correlation;
            }

            return context.TraceIdentifier;
        }
    }

    /// <inheritdoc />
    public string? TraceId => Activity.Current?.TraceId.ToString();
}
