using NetworkMonitoring.Probe.Application.Ports;

namespace NetworkMonitoring.Probe.IntegrationTests.Support;

internal sealed class NoOpProbeFlowTelemetry : IProbeFlowTelemetry
{
    public static NoOpProbeFlowTelemetry Instance { get; } = new();
    public void TrackPacketReceived() { }
    public void TrackCaptureDropped(long count) { }
    public void TrackUnparsableInput() { }
    public void TrackSessionDetected() { }
    public void TrackDeviceDiscovered() { }
}
