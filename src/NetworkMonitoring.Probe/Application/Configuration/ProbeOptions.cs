namespace NetworkMonitoring.Probe.Application.Configuration;

public sealed class ProbeOptions
{
    public const string SectionName = "Probe";

    public string TSharkPath { get; init; } = "tshark";

    public string InterfaceName { get; init; } = "eth0";

    public string CaptureFilter { get; init; } = string.Empty;

    public int SessionDeduplicationWindowMinutes { get; init; } = 10;

    public int DeviceDeduplicationWindowMinutes { get; init; } = 10;
}
