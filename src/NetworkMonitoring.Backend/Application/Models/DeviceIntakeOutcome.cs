namespace NetworkMonitoring.Backend.Application.Models;

/// <summary>
/// Specifies the possible outcomes of a device intake operation.
/// </summary>
public enum DeviceIntakeOutcomeKind
{
    /// <summary>
    /// A new device was successfully created in the inventory.
    /// </summary>
    Created,

    /// <summary>
    /// An existing device was successfully updated in the inventory.
    /// </summary>
    Updated,

    /// <summary>
    /// The intake request was identified as a duplicate and no changes were made.
    /// </summary>
    Idempotent,

    /// <summary>
    /// The intake request was rejected due to validation errors.
    /// </summary>
    Rejected,

    /// <summary>
    /// The intake operation failed due to a persistence error.
    /// </summary>
    PersistenceFailure
}

/// <summary>
/// Represents the outcome of a device intake operation.
/// </summary>
/// <param name="Kind">The kind of outcome.</param>
/// <param name="Device">The device involved in the operation, if applicable.</param>
/// <param name="Reason">A description of the outcome, especially in case of failure.</param>
/// <param name="StatusCode">An optional status code associated with the outcome.</param>
public sealed record DeviceIntakeOutcome(
    DeviceIntakeOutcomeKind Kind,
    DeviceInventoryItem? Device,
    string? Reason,
    int? StatusCode = null)
{
    /// <summary>
    /// Creates an outcome indicating that a device was successfully created.
    /// </summary>
    /// <param name="device">The created device.</param>
    /// <returns>A new <see cref="DeviceIntakeOutcome"/> instance.</returns>
    public static DeviceIntakeOutcome Created(DeviceInventoryItem device) =>
        new(DeviceIntakeOutcomeKind.Created, device, null);

    /// <summary>
    /// Creates an outcome indicating that a device was successfully updated.
    /// </summary>
    /// <param name="device">The updated device.</param>
    /// <returns>A new <see cref="DeviceIntakeOutcome"/> instance.</returns>
    public static DeviceIntakeOutcome Updated(DeviceInventoryItem device) =>
        new(DeviceIntakeOutcomeKind.Updated, device, null);

    /// <summary>
    /// Creates an outcome indicating that the intake request was idempotent.
    /// </summary>
    /// <param name="device">The existing device.</param>
    /// <returns>A new <see cref="DeviceIntakeOutcome"/> instance.</returns>
    public static DeviceIntakeOutcome Idempotent(DeviceInventoryItem device) =>
        new(DeviceIntakeOutcomeKind.Idempotent, device, null);

    /// <summary>
    /// Creates an outcome indicating that the intake request was rejected.
    /// </summary>
    /// <param name="reason">The reason for rejection.</param>
    /// <param name="statusCode">The status code for rejection (default 400).</param>
    /// <returns>A new <see cref="DeviceIntakeOutcome"/> instance.</returns>
    public static DeviceIntakeOutcome Rejected(string reason, int statusCode = 400) =>
        new(DeviceIntakeOutcomeKind.Rejected, null, reason, statusCode);

    /// <summary>
    /// Creates an outcome indicating a persistence failure.
    /// </summary>
    /// <param name="reason">The reason for the failure.</param>
    /// <returns>A new <see cref="DeviceIntakeOutcome"/> instance.</returns>
    public static DeviceIntakeOutcome PersistenceFailure(string reason) =>
        new(DeviceIntakeOutcomeKind.PersistenceFailure, null, reason, 503);
}
