using System.Net;
using NetworkMonitoring.IntegrationConsole.Application.Models;

namespace NetworkMonitoring.IntegrationConsole.Infrastructure.Backend;

public sealed class DeviceIntakeRetryPolicy
{
    public bool IsRetryable(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.RequestTimeout
            or (HttpStatusCode)429
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;

    public bool IsPermanentRejection(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity;

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
