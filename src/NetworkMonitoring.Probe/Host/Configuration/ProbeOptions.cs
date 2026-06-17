using NetworkMonitoring.Probe.Application.Configuration;

namespace NetworkMonitoring.Probe.Host.Configuration;

/// <summary>
/// Provides registration metadata for probe configuration.
/// </summary>
public static class ProbeOptionsRegistration
{
    /// <summary>
    /// The section name used in configuration files.
    /// </summary>
    public static string SectionName => ProbeOptions.SectionName;
}
