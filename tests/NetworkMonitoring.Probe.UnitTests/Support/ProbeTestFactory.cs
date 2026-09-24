using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Probe.Application.Configuration;
using NetworkMonitoring.Probe.Application.Ports;
using NetworkMonitoring.Probe.Application.UseCases;

namespace NetworkMonitoring.Probe.UnitTests.Support;

/// <summary>
/// Single construction point for probe use cases under test, so a constructor signature change
/// is absorbed here instead of in every test.
/// </summary>
internal static class ProbeTestFactory
{
    public static ProcessObservationsUseCase CreateUseCase(
        ITrafficProvider provider,
        IMessagePublisher publisher,
        IProbeFlowTelemetry? telemetry = null,
        int sessionDeduplicationWindowMinutes = 0,
        int deviceDeduplicationWindowMinutes = 0) =>
        new(
            provider,
            publisher,
            telemetry ?? NoOpProbeFlowTelemetry.Instance,
            Options.Create(
                new ProbeOptions
                {
                    SessionDeduplicationWindowMinutes = sessionDeduplicationWindowMinutes,
                    DeviceDeduplicationWindowMinutes = deviceDeduplicationWindowMinutes
                }),
            NullLogger<ProcessObservationsUseCase>.Instance);
}
