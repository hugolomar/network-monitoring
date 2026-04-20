namespace NetworkMonitoring.Probe.Application.Models;

public sealed record DiscoveryValidationResult(bool IsValid, IReadOnlyCollection<string> Errors)
{
    public static DiscoveryValidationResult Valid() => new(true, Array.Empty<string>());

    public static DiscoveryValidationResult Invalid(params string[] errors) => new(false, errors);
}
