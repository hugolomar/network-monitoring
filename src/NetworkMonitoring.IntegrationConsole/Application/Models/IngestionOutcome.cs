namespace NetworkMonitoring.IntegrationConsole.Application.Models;

/// <summary>
/// Defines the possible kinds of ingestion outcomes.
/// </summary>
public enum IngestionOutcomeKind
{
    /// <summary>The event was successfully ingested.</summary>
    Succeeded,
    /// <summary>The ingestion failed but can be retried.</summary>
    RetryableFailure,
    /// <summary>The event was rejected by the backend as invalid or duplicate.</summary>
    Rejected,
    /// <summary>The ingestion failed after all retry attempts were exhausted.</summary>
    RetryExhausted
}

/// <summary>
/// Represents the result of an ingestion attempt.
/// </summary>
/// <param name="Kind">The kind of outcome.</param>
/// <param name="AttemptCount">Number of attempts made.</param>
/// <param name="StatusCode">HTTP status code if applicable.</param>
/// <param name="Reason">Human-readable reason for the outcome.</param>
public sealed record IngestionOutcome(
    IngestionOutcomeKind Kind,
    int AttemptCount,
    int? StatusCode,
    string Reason)
{
    /// <summary>Creates a successful outcome.</summary>
    public static IngestionOutcome Succeeded(int attemptCount = 1, int? statusCode = null, string reason = "Accepted") =>
        new(IngestionOutcomeKind.Succeeded, attemptCount, statusCode, reason);

    /// <summary>Creates a retryable failure outcome.</summary>
    public static IngestionOutcome RetryableFailure(int attemptCount, int? statusCode, string reason) =>
        new(IngestionOutcomeKind.RetryableFailure, attemptCount, statusCode, reason);

    /// <summary>Creates a rejected outcome.</summary>
    public static IngestionOutcome Rejected(string reason, int? statusCode = null, int attemptCount = 1) =>
        new(IngestionOutcomeKind.Rejected, attemptCount, statusCode, reason);

    /// <summary>Creates a retry exhausted outcome.</summary>
    public static IngestionOutcome RetryExhausted(int attemptCount, int? statusCode, string reason) =>
        new(IngestionOutcomeKind.RetryExhausted, attemptCount, statusCode, reason);
}

/// <summary>
/// Represents an event that was rejected and might be sent to a dead-letter queue.
/// </summary>
/// <param name="Reason">The reason for rejection.</param>
/// <param name="Topic">Kafka topic where the event originated.</param>
/// <param name="Partition">Kafka partition.</param>
/// <param name="Offset">Kafka offset.</param>
/// <param name="MacAddress">MAC address of the device in the event.</param>
public sealed record RejectedEvent(
    string Reason,
    string? Topic = null,
    int? Partition = null,
    long? Offset = null,
    string? MacAddress = null);
