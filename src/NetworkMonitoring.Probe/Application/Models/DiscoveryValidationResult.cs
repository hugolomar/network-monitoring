namespace NetworkMonitoring.Probe.Application.Models;

/// <summary>
/// Represents the outcome of validating a device discovery event.
/// </summary>
/// <param name="IsValid">True if the discovery data meets all validation criteria; otherwise, false.</param>
/// <param name="Errors">A collection of error messages detailing why the discovery is invalid.</param>
public sealed record DiscoveryValidationResult(bool IsValid, IReadOnlyCollection<string> Errors)
{
    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A valid <see cref="DiscoveryValidationResult"/>.</returns>
    public static DiscoveryValidationResult Valid() => new(true, Array.Empty<string>());

    /// <summary>
    /// Creates a failed validation result with specific error messages.
    /// </summary>
    /// <param name="errors">The list of validation errors.</param>
    /// <returns>An invalid <see cref="DiscoveryValidationResult"/>.</returns>
    public static DiscoveryValidationResult Invalid(params string[] errors) => new(false, errors);
}
