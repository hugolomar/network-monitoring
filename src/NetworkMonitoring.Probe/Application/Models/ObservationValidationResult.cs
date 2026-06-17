namespace NetworkMonitoring.Probe.Application.Models;

/// <summary>
/// Represents the outcome of validating a traffic observation.
/// </summary>
/// <param name="IsValid">True if the observation meets all validation criteria; otherwise, false.</param>
/// <param name="Errors">A collection of error messages detailing why the observation is invalid.</param>
public sealed record ObservationValidationResult(bool IsValid, IReadOnlyCollection<string> Errors)
{
    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A valid <see cref="ObservationValidationResult"/>.</returns>
    public static ObservationValidationResult Valid() => new(true, Array.Empty<string>());

    /// <summary>
    /// Creates a failed validation result with specific error messages.
    /// </summary>
    /// <param name="errors">The list of validation errors.</param>
    /// <returns>An invalid <see cref="ObservationValidationResult"/>.</returns>
    public static ObservationValidationResult Invalid(params string[] errors) => new(false, errors);
}
