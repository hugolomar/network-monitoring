using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Probe.Application.Configuration;
using NetworkMonitoring.Probe.Application.Models;
using NetworkMonitoring.Probe.Application.Ports;
using NetworkMonitoring.Probe.Application.UseCases;

namespace NetworkMonitoring.Probe.UnitTests.Application.UseCases;

public sealed class ProcessObservationsUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WithInvalidThenValidObservation_SkipsInvalidAndContinues()
    {
        var provider = new FakeTrafficProvider(
        [
            new TrafficObservation(
                "invalid-ip",
                "10.0.0.2",
                1234,
                443,
                "6",
                DateTimeOffset.UtcNow,
                100,
                "AA:BB:CC:DD:EE:FF",
                "11:22:33:44:55:66",
                "invalid-source",
                "traffic"),
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

        var publisher = new RecordingPublisher();
        var useCase = CreateUseCase(provider, publisher);

        await useCase.ExecuteAsync(CancellationToken.None);

        Assert.Equal(1, publisher.SessionDetectedCount);
        Assert.Equal(2, publisher.DeviceDetectedCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidMacEvidence_RejectsInvalidDiscoveryAndContinues()
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
                100,
                "invalid-mac",
                "11:22:33:44:55:66",
                "host-a",
                "traffic")
        ]);

        var publisher = new RecordingPublisher();
        var useCase = CreateUseCase(provider, publisher);

        await useCase.ExecuteAsync(CancellationToken.None);

        Assert.Equal(1, publisher.SessionDetectedCount);
        Assert.Single(publisher.Devices);
        Assert.Equal("11:22:33:44:55:66", publisher.Devices[0].MacAddress.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WithRepeatedDeviceDetection_ConsolidatesLifecycleTimestamps()
    {
        var firstSeen = new DateTimeOffset(2026, 4, 10, 10, 0, 0, TimeSpan.Zero);
        var secondSeen = firstSeen.AddMinutes(2);
        var provider = new FakeTrafficProvider(
        [
            new TrafficObservation(
                "10.0.0.1",
                "10.0.0.2",
                1234,
                443,
                "6",
                firstSeen,
                100,
                "aa:bb:cc:dd:ee:ff",
                null,
                "host-a",
                "arp"),
            new TrafficObservation(
                "10.0.0.1",
                "10.0.0.3",
                1234,
                443,
                "6",
                secondSeen,
                120,
                "aa:bb:cc:dd:ee:ff",
                null,
                "host-b",
                "arp")
        ]);

        var publisher = new RecordingPublisher();
        var useCase = CreateUseCase(provider, publisher);

        await useCase.ExecuteAsync(CancellationToken.None);

        Assert.Equal(2, publisher.DeviceDetectedCount);
        var firstEmission = publisher.Devices[0];
        var secondEmission = publisher.Devices[1];
        Assert.Null(firstEmission.Id);
        Assert.Null(secondEmission.Id);
        Assert.Equal(firstSeen, secondEmission.FirstSeenUtc);
        Assert.Equal(secondSeen, secondEmission.LastSeenUtc);
        Assert.Contains(secondEmission.ObservedIps, ip => ip.Value == "10.0.0.1");
    }

    [Fact]
    public async Task ExecuteAsync_WithRepeatedDeviceWithinDedupWindow_SuppressesExtraDeviceEmission()
    {
        var t0 = new DateTimeOffset(2026, 4, 10, 10, 0, 0, TimeSpan.Zero);
        var t1 = t0.AddMinutes(1);
        var provider = new FakeTrafficProvider(
        [
            new TrafficObservation(
                "10.0.0.1",
                "10.0.0.2",
                1234,
                443,
                "6",
                t0,
                100,
                "aa:bb:cc:dd:ee:ff",
                null,
                "host-a",
                "arp"),
            new TrafficObservation(
                "10.0.0.1",
                "10.0.0.3",
                1234,
                443,
                "6",
                t1,
                120,
                "aa:bb:cc:dd:ee:ff",
                null,
                "host-b",
                "arp")
        ]);

        var publisher = new RecordingPublisher();
        var options = Options.Create(new ProbeOptions
        {
            SessionDeduplicationWindowMinutes = 0,
            DeviceDeduplicationWindowMinutes = 10
        });
        var useCase = new ProcessObservationsUseCase(
            provider,
            publisher,
            options,
            NullLogger<ProcessObservationsUseCase>.Instance);

        await useCase.ExecuteAsync(CancellationToken.None);

        Assert.Equal(1, publisher.DeviceDetectedCount);
        Assert.Single(publisher.Devices);
        Assert.Equal(t0, publisher.Devices[0].FirstSeenUtc);
        Assert.Equal(t1, publisher.Devices[0].LastSeenUtc);
    }

    private static ProcessObservationsUseCase CreateUseCase(
        ITrafficProvider provider,
        IMessagePublisher publisher)
    {
        var options = Options.Create(new ProbeOptions
        {
            SessionDeduplicationWindowMinutes = 0,
            DeviceDeduplicationWindowMinutes = 0
        });

        return new ProcessObservationsUseCase(
            provider,
            publisher,
            options,
            NullLogger<ProcessObservationsUseCase>.Instance);
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
        public int SessionDetectedCount { get; private set; }
        public int DeviceDetectedCount { get; private set; }
        public List<Device> Devices { get; } = [];

        public Task PublishSessionDetected(Session session, CancellationToken cancellationToken)
        {
            SessionDetectedCount++;
            return Task.CompletedTask;
        }

        public Task PublishDeviceDetected(Device device, CancellationToken cancellationToken)
        {
            DeviceDetectedCount++;
            Devices.Add(device);
            return Task.CompletedTask;
        }
    }
}
