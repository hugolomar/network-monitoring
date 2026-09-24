namespace NetworkMonitoring.Probe.Application.Ports;

/// <summary>
/// Emits pipeline-stage and capture-loss metrics for the probe.
/// </summary>
public interface IProbeFlowTelemetry
{
    /// <summary>Increments the count of packets that became observations.</summary>
    void TrackPacketReceived();

    /// <summary>Increments capture drops reported by the capture process.</summary>
    /// <param name="count">Number of packets dropped.</param>
    void TrackCaptureDropped(long count);

    /// <summary>Increments inputs that could not be mapped into an observation.</summary>
    void TrackUnparsableInput();

    /// <summary>Increments sessions published after validation and deduplication.</summary>
    void TrackSessionDetected();

    /// <summary>Increments devices published after validation and deduplication.</summary>
    void TrackDeviceDiscovered();
}
