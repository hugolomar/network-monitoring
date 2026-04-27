namespace NetworkMonitoring.Backend.Application.Models;

public enum DeviceIntakeOutcomeKind
{
    Created,
    Updated,
    Idempotent,
    Rejected,
    PersistenceFailure
}

public sealed record DeviceIntakeOutcome(
    DeviceIntakeOutcomeKind Kind,
    DeviceInventoryItem? Device,
    string? Reason,
    int? StatusCode = null)
{
    public static DeviceIntakeOutcome Created(DeviceInventoryItem device) =>
        new(DeviceIntakeOutcomeKind.Created, device, null);

    public static DeviceIntakeOutcome Updated(DeviceInventoryItem device) =>
        new(DeviceIntakeOutcomeKind.Updated, device, null);

    public static DeviceIntakeOutcome Idempotent(DeviceInventoryItem device) =>
        new(DeviceIntakeOutcomeKind.Idempotent, device, null);

    public static DeviceIntakeOutcome Rejected(string reason, int statusCode = 400) =>
        new(DeviceIntakeOutcomeKind.Rejected, null, reason, statusCode);

    public static DeviceIntakeOutcome PersistenceFailure(string reason) =>
        new(DeviceIntakeOutcomeKind.PersistenceFailure, null, reason, 503);
}
