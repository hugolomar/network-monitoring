using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Probe.Application.Configuration;
using NetworkMonitoring.Probe.Application.Models;
using NetworkMonitoring.Probe.Application.Ports;
using NetworkMonitoring.Probe.Application.UseCases;
using NetworkMonitoring.Probe.Infrastructure.Traffic;

namespace NetworkMonitoring.Probe.UnitTests.Observability;

/// <summary>
/// Verifies capture-loss and throughput metric emission points on the probe (FR-015, FR-016).
/// </summary>
public sealed class CaptureLossMetricTests
{
    /// <summary>
    /// Ensures published sessions and devices increment the corresponding counters.
    /// </summary>
    [Fact]
    public async Task ProcessObservations_emits_session_and_device_throughput_counters()
    {
        var telemetry = new RecordingProbeFlowTelemetry();
        var provider = new FakeTrafficProvider(
        [
            new TrafficObservation(
                "10.0.0.1",
                "10.0.0.2",
                1234,
                443,
                "6",
                DateTimeOffset.UtcNow,
                200,
                "AA:BB:CC:DD:EE:FF",
                "11:22:33:44:55:66",
                "host-a",
                "arp")
        ]);
        var useCase = new ProcessObservationsUseCase(
            provider,
            new RecordingPublisher(),
            telemetry,
            Options.Create(new ProbeOptions
            {
                SessionDeduplicationWindowMinutes = 0,
                DeviceDeduplicationWindowMinutes = 0
            }),
            NullLogger<ProcessObservationsUseCase>.Instance);

        await useCase.ExecuteAsync(CancellationToken.None);

        Assert.Equal(1, telemetry.SessionsDetected);
        Assert.Equal(2, telemetry.DevicesDiscovered);
    }

    /// <summary>
    /// Ensures tshark drop lines are parsed into a positive capture-drop count.
    /// </summary>
    [Theory]
    [InlineData("12 packets dropped from capture", 12L)]
    [InlineData("1 packet dropped", 1L)]
    [InlineData("Packets received: 100", null)]
    public void TsharkCaptureDiagnostics_reads_drop_counts(string line, long? expected)
    {
        var parsed = TsharkCaptureDiagnostics.TryReadCaptureDrops(line, out var count);
        if (expected is null)
        {
            Assert.False(parsed);
            return;
        }

        Assert.True(parsed);
        Assert.Equal(expected.Value, count);
    }

    private sealed class RecordingProbeFlowTelemetry : IProbeFlowTelemetry
    {
        public int SessionsDetected { get; private set; }
        public int DevicesDiscovered { get; private set; }

        public void TrackPacketReceived() { }
        public void TrackCaptureDropped(long count) { }
        public void TrackUnparsableInput() { }
        public void TrackSessionDetected() => SessionsDetected++;
        public void TrackDeviceDiscovered() => DevicesDiscovered++;
    }

    private sealed class FakeTrafficProvider(IReadOnlyList<TrafficObservation> observations) : ITrafficProvider
    {
        public async IAsyncEnumerable<TrafficObservation> ReadObservations(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (var observation in observations)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
                yield return observation;
            }
        }
    }

    private sealed class RecordingPublisher : IMessagePublisher
    {
        public Task PublishSessionDetected(Session session, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task PublishDeviceDetected(Device device, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
