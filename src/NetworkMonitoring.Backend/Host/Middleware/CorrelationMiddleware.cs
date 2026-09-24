using System.Diagnostics;
using Microsoft.Extensions.Primitives;

namespace NetworkMonitoring.Backend.Host.Middleware;

/// <summary>
/// Adds and propagates correlation identifiers for backend requests.
/// </summary>
public sealed class CorrelationMiddleware(
    RequestDelegate next,
    ILogger<CorrelationMiddleware> logger)
{
    private const string CorrelationHeader = "X-Correlation-ID";
    private const string TraceParentHeader = "traceparent";

    /// <summary>
    /// Handles an HTTP request and ensures correlation context is available.
    /// </summary>
    public async Task Invoke(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationHeader].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        }

        context.TraceIdentifier = correlationId;
        context.Items[CorrelationHeader] = correlationId;
        context.Response.Headers[CorrelationHeader] = correlationId;

        if (Activity.Current is not null)
        {
            Activity.Current.SetTag("correlation.id", correlationId);
            context.Response.Headers[TraceParentHeader] = Activity.Current.Id;
        }

        using (logger.BeginScope(new Dictionary<string, object?>
        {
            ["correlationId"] = correlationId,
            ["traceId"] = Activity.Current?.TraceId.ToString()
        }))
        {
            await next(context);
        }
    }
}
