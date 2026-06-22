namespace NetworkMonitoring.Probe.Application.Configuration;

/// <summary>
/// Defines the supported traffic source modes for the probe.
/// </summary>
public enum CaptureInputMode
{
    /// <summary>
    /// Captures traffic from a live network interface via tshark.
    /// </summary>
    Live = 0,

    /// <summary>
    /// Reads traffic deterministically from a configured PCAP file for tests/validation.
    /// </summary>
    DeterministicTest = 1,
}
