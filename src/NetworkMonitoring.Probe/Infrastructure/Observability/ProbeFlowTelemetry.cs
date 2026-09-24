using System.Diagnostics.Metrics;
using NetworkMonitoring.Probe.Application.Ports;

namespace NetworkMonitoring.Probe.Infrastructure.Observability;

/// <summary>
/// OpenTelemetry-backed probe flow metrics (FR-015, FR-016).
/// </summary>
public sealed class ProbeFlowTelemetry : IProbeFlowTelemetry
{
    private static readonly Meter Meter = new("NetworkMonitoring.Probe");
    private static readonly Counter<long> PacketsReceived = Meter.CreateCounter<long>("packets_received_total");
    private static readonly Counter<long> CaptureDropped = Meter.CreateCounter<long>("capture_dropped_total");
    private static readonly Counter<long> UnparsableInput = Meter.CreateCounter<long>("unparsable_input_total");
    private static readonly Counter<long> SessionsDetected = Meter.CreateCounter<long>("sessions_detected_total");
    private static readonly Counter<long> DevicesDiscovered = Meter.CreateCounter<long>("devices_discovered_total");

    /// <inheritdoc />
    public void TrackPacketReceived() => PacketsReceived.Add(1);

    /// <inheritdoc />
    public void TrackCaptureDropped(long count)
    {
        if (count > 0)
        {
            CaptureDropped.Add(count);
        }
    }

    /// <inheritdoc />
    public void TrackUnparsableInput() => UnparsableInput.Add(1);

    /// <inheritdoc />
    public void TrackSessionDetected() => SessionsDetected.Add(1);

    /// <inheritdoc />
    public void TrackDeviceDiscovered() => DevicesDiscovered.Add(1);
}
