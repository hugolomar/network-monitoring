using NetworkMonitoring.Backend.Application.Ports;

namespace NetworkMonitoring.Backend.UnitTests.Support;

internal sealed class NoOpIntakeFlowTelemetry : IIntakeFlowTelemetry
{
    public static NoOpIntakeFlowTelemetry Instance { get; } = new();
    public void TrackFreshness(long freshnessMs, string outcome) { }
}
