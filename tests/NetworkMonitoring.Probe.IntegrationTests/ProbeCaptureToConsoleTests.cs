using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Probe.Application.Configuration;
using NetworkMonitoring.Probe.Application.Models;
using NetworkMonitoring.Probe.Application.Ports;
using NetworkMonitoring.Probe.Application.UseCases;
using NetworkMonitoring.Probe.Infrastructure.Publishing;

namespace NetworkMonitoring.Probe.IntegrationTests;

public sealed class ProbeCaptureToConsoleTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidObservation_WritesSessionAndDeviceEvents()
    {
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

        var serializer = new ConsoleRecordSerializer();
        var publisher = new ConsolePublisher(serializer);
        var options = Options.Create(new ProbeOptions
        {
            SessionDeduplicationWindowMinutes = 0,
            DeviceDeduplicationWindowMinutes = 0
        });
        var useCase = new ProcessObservationsUseCase(
            provider,
            publisher,
            options,
            NullLogger<ProcessObservationsUseCase>.Instance);

        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            await useCase.ExecuteAsync(CancellationToken.None);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        var output = writer.ToString();

        Assert.Contains("SessionDetected", output);
        Assert.Contains("DeviceDetected", output);
    }

    [Fact]
    public async Task ExecuteAsync_WithRepeatedDeviceEvidence_EmitsConsolidatedDeviceTimeline()
    {
        var firstSeen = new DateTimeOffset(2026, 4, 10, 10, 0, 0, TimeSpan.Zero);
        var secondSeen = firstSeen.AddMinutes(1);
        var provider = new FakeTrafficProvider(
        [
            new TrafficObservation(
                "10.0.0.1",
                "10.0.0.2",
                1234,
                443,
                "6",
                firstSeen,
                200,
                "AA:BB:CC:DD:EE:FF",
                null,
                "host-a",
                "arp"),
            new TrafficObservation(
                "10.0.0.3",
                "10.0.0.4",
                1234,
                443,
                "6",
                secondSeen,
                220,
                "AA:BB:CC:DD:EE:FF",
                null,
                "host-b",
                "arp")
        ]);

        var serializer = new ConsoleRecordSerializer();
        var publisher = new ConsolePublisher(serializer);
        var options = Options.Create(new ProbeOptions
        {
            SessionDeduplicationWindowMinutes = 0,
            DeviceDeduplicationWindowMinutes = 0
        });
        var useCase = new ProcessObservationsUseCase(
            provider,
            publisher,
            options,
            NullLogger<ProcessObservationsUseCase>.Instance);

        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            await useCase.ExecuteAsync(CancellationToken.None);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        var output = writer.ToString();

        Assert.Contains("\"eventType\":\"DeviceDetected\"", output);
        Assert.Contains("\"firstSeenUtc\":\"2026-04-10T10:00:00+00:00\"", output);
        Assert.Contains("\"lastSeenUtc\":\"2026-04-10T10:01:00+00:00\"", output);
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
}
