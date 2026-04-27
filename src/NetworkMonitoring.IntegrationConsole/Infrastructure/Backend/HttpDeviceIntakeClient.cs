using System.Net;
using System.Net.Http.Json;
using NetworkMonitoring.IntegrationConsole.Application.Configuration;
using NetworkMonitoring.IntegrationConsole.Application.Models;
using NetworkMonitoring.IntegrationConsole.Application.Ports;

namespace NetworkMonitoring.IntegrationConsole.Infrastructure.Backend;

public sealed class HttpDeviceIntakeClient(
    HttpClient httpClient,
    RetryOptions retryOptions,
    DeviceIntakeRetryPolicy retryPolicy) : IDeviceIntakeClient
{
    public async Task<IngestionOutcome> Send(DeviceDetectedEvent detectedEvent, CancellationToken cancellationToken)
    {
        var requestBody = DeviceIntakeRequestMapper.Map(detectedEvent);
        var delay = retryOptions.BaseDelay;

        for (var attempt = 1; attempt <= retryOptions.MaxAttempts; attempt++)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "/devices")
                {
                    Content = JsonContent.Create(requestBody)
                };
                request.Headers.Add("Idempotency-Key", detectedEvent.MacAddress);

                using var response = await httpClient.SendAsync(request, cancellationToken);
                var reason = $"{(int)response.StatusCode} {response.ReasonPhrase}".Trim();

                if (response.IsSuccessStatusCode || retryPolicy.IsPermanentRejection(response.StatusCode) || response.StatusCode is HttpStatusCode.Conflict)
                {
                    return retryPolicy.ClassifyFinal(response.StatusCode, attempt, reason);
                }

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
                if (attempt == retryOptions.MaxAttempts)
                {
                    return IngestionOutcome.RetryExhausted(attempt, null, ex.Message);
                }
            }

            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
            }
        }

        return IngestionOutcome.RetryExhausted(retryOptions.MaxAttempts, null, "Retry attempts exhausted");
    }
}
