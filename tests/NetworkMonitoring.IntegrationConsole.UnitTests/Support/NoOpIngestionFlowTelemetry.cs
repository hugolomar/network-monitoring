using NetworkMonitoring.IntegrationConsole.Application.Ports;

namespace NetworkMonitoring.IntegrationConsole.UnitTests.Support;

internal sealed class NoOpIngestionFlowTelemetry : IIngestionFlowTelemetry
{
    public static NoOpIngestionFlowTelemetry Instance { get; } = new();
    public void TrackIngestion(string outcome) { }
    public void TrackConsumerLag(string topic, int partition, long lag) { }
}
