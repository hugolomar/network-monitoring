namespace NetworkMonitoring.IntegrationConsole.Application.Models;

public enum IngestionOutcomeKind
{
    Succeeded,
    RetryableFailure,
    Rejected,
    RetryExhausted
}

public sealed record IngestionOutcome(
    IngestionOutcomeKind Kind,
    int AttemptCount,
    int? StatusCode,
    string Reason)
{
    public static IngestionOutcome Succeeded(int attemptCount = 1, int? statusCode = null, string reason = "Accepted") =>
        new(IngestionOutcomeKind.Succeeded, attemptCount, statusCode, reason);

    public static IngestionOutcome RetryableFailure(int attemptCount, int? statusCode, string reason) =>
        new(IngestionOutcomeKind.RetryableFailure, attemptCount, statusCode, reason);

    public static IngestionOutcome Rejected(string reason, int? statusCode = null, int attemptCount = 1) =>
        new(IngestionOutcomeKind.Rejected, attemptCount, statusCode, reason);

    public static IngestionOutcome RetryExhausted(int attemptCount, int? statusCode, string reason) =>
        new(IngestionOutcomeKind.RetryExhausted, attemptCount, statusCode, reason);
}

public sealed record RejectedEvent(
    string Reason,
    string? Topic = null,
    int? Partition = null,
    long? Offset = null,
    string? MacAddress = null);
