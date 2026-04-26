namespace NetworkMonitoring.Probe.Application.Models;

public sealed record ObservationValidationResult(bool IsValid, IReadOnlyCollection<string> Errors)
{
    public static ObservationValidationResult Valid() => new(true, Array.Empty<string>());

    public static ObservationValidationResult Invalid(params string[] errors) => new(false, errors);
}
