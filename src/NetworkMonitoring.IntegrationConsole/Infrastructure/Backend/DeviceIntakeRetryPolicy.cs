using System.Net;
using NetworkMonitoring.IntegrationConsole.Application.Models;

namespace NetworkMonitoring.IntegrationConsole.Infrastructure.Backend;

/// <summary>
/// Policy defining retryable and permanent failure conditions for device intake requests.
/// </summary>
public sealed class DeviceIntakeRetryPolicy
{
    /// <summary>
    /// Determines if a status code represents a transient failure that should be retried.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>True if retryable, false otherwise.</returns>
    public bool IsRetryable(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.RequestTimeout
            or (HttpStatusCode)429
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;

    /// <summary>
    /// Determines if a status code represents a permanent rejection that should not be retried.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>True if permanent, false otherwise.</returns>
    public bool IsPermanentRejection(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity;

    /// <summary>
    /// Classifies an HTTP response into a final <see cref="IngestionOutcome"/>.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="attemptCount">Total attempts made.</param>
    /// <param name="reason">Human-readable reason.</param>
    /// <returns>The classified outcome.</returns>
    public IngestionOutcome ClassifyFinal(HttpStatusCode statusCode, int attemptCount, string reason)
    {
        if ((int)statusCode >= 200 && (int)statusCode <= 299)
        {
            return IngestionOutcome.Succeeded(attemptCount, (int)statusCode);
        }

        if (IsPermanentRejection(statusCode) || statusCode is HttpStatusCode.Conflict)
        {
            return IngestionOutcome.Rejected(reason, (int)statusCode, attemptCount);
        }

        return IngestionOutcome.RetryExhausted(attemptCount, (int)statusCode, reason);
    }
}
