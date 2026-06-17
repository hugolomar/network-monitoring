using System.Net;
using System.Net.Http.Json;
using NetworkMonitoring.IntegrationConsole.Application.Configuration;
using NetworkMonitoring.IntegrationConsole.Application.Models;
using NetworkMonitoring.IntegrationConsole.Application.Ports;

namespace NetworkMonitoring.IntegrationConsole.Infrastructure.Backend;

/// <summary>
/// HTTP client implementation for sending device intake requests to the Backend API.
/// </summary>
/// <param name="httpClient">The HTTP client to use for requests.</param>
/// <param name="retryOptions">Configuration for retry logic.</param>
/// <param name="retryPolicy">Policy determining which outcomes are retryable.</param>
public sealed class HttpDeviceIntakeClient(
    HttpClient httpClient,
    RetryOptions retryOptions,
    DeviceIntakeRetryPolicy retryPolicy) : IDeviceIntakeClient
{
    /// <summary>
    /// Sends a device detected event to the backend.
    /// </summary>
    /// <param name="detectedEvent">The device event to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The outcome of the ingestion process.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public async Task<IngestionOutcome> Send(DeviceDetectedEvent detectedEvent, CancellationToken cancellationToken)
    {
        var requestBody = DeviceIntakeRequestMapper.Map(detectedEvent);
        var delay = retryOptions.BaseDelay;

        // Implementation of a manual retry loop to handle transient failures 
        // while respecting idempotency and specific backend business responses.
        for (var attempt = 1; attempt <= retryOptions.MaxAttempts; attempt++)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "/devices")
                {
                    Content = JsonContent.Create(requestBody)
                };
                // We use the MAC address as an idempotency key to ensure that 
                // re-processing the same detection doesn't create duplicate inventory records.
                request.Headers.Add("Idempotency-Key", detectedEvent.MacAddress);

                using var response = await httpClient.SendAsync(request, cancellationToken);
                var reason = $"{(int)response.StatusCode} {response.ReasonPhrase}".Trim();

                // Successful responses (2xx), permanent rejections (e.g. 400 Bad Request),
                // and conflicts (409) are considered terminal states for the retry loop.
                if (response.IsSuccessStatusCode || retryPolicy.IsPermanentRejection(response.StatusCode) || response.StatusCode is HttpStatusCode.Conflict)
                {
                    return retryPolicy.ClassifyFinal(response.StatusCode, attempt, reason);
                }

                // If the status code is not retryable (according to policy) or we reached max attempts,
                // we stop retrying and return the final classification.
                if (!retryPolicy.IsRetryable(response.StatusCode) || attempt == retryOptions.MaxAttempts)
                {
                    return retryPolicy.ClassifyFinal(response.StatusCode, attempt, reason);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Network errors or timeouts are treated as transient until max attempts are reached.
                if (attempt == retryOptions.MaxAttempts)
                {
                    return IngestionOutcome.RetryExhausted(attempt, null, ex.Message);
                }
            }

            // Exponential backoff or fixed delay between retry attempts to avoid hammering the backend.
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
            }
        }

        return IngestionOutcome.RetryExhausted(retryOptions.MaxAttempts, null, "Retry attempts exhausted");
    }
}
