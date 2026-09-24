using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Probe.Application.Configuration;
using NetworkMonitoring.Probe.Application.Ports;
using NetworkMonitoring.Probe.Application.UseCases;
using NetworkMonitoring.Probe.Infrastructure.Traffic;

namespace NetworkMonitoring.Probe.IntegrationTests.Support;

/// <summary>
/// Single construction point for probe components under test, so a constructor signature change
/// is absorbed here instead of in every test.
/// </summary>
internal static class ProbeTestFactory
{
    public static PcapFileTrafficProvider CreatePcapProvider(IOptions<ProbeOptions> options) =>
        new(
            options,
            new TsharkObservationMapper(),
            NoOpProbeFlowTelemetry.Instance,
            NullLogger<PcapFileTrafficProvider>.Instance);

    public static ProcessObservationsUseCase CreateUseCase(
        ITrafficProvider provider,
        IMessagePublisher publisher,
        IOptions<ProbeOptions> options) =>
        new(
            provider,
            publisher,
            NoOpProbeFlowTelemetry.Instance,
            options,
            NullLogger<ProcessObservationsUseCase>.Instance);
}
