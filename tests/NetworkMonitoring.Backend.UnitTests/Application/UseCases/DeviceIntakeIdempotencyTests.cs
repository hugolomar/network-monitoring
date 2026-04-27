using NetworkMonitoring.Backend.Application.Models;

namespace NetworkMonitoring.Backend.UnitTests.Application.UseCases;

public sealed class DeviceIntakeIdempotencyTests
{
    [Fact]
    public async Task Execute_treats_exact_duplicate_as_idempotent()
    {
        var repository = new Support.InMemoryDeviceInventoryRepository();
        var useCase = CreateUseCase(repository);

        await useCase.Execute(AcceptDeviceIntakeUseCaseTests.ValidCommand(), CancellationToken.None);
        var duplicate = await useCase.Execute(AcceptDeviceIntakeUseCaseTests.ValidCommand(), CancellationToken.None);

        Assert.Equal(DeviceIntakeOutcomeKind.Idempotent, duplicate.Kind);
    }

    [Fact]
    public async Task Execute_consolidates_timestamps_observed_ips_hostname_and_primary_ip()
    {
        var repository = new Support.InMemoryDeviceInventoryRepository();
        var useCase = CreateUseCase(repository);

        await useCase.Execute(AcceptDeviceIntakeUseCaseTests.ValidCommand(
            primaryIp: null,
            hostname: null,
            observedIps: ["192.168.1.10"],
            firstSeenUtc: DateTimeOffset.Parse("2026-04-27T12:05:00Z"),
            lastSeenUtc: DateTimeOffset.Parse("2026-04-27T12:10:00Z")), CancellationToken.None);

        var updated = await useCase.Execute(AcceptDeviceIntakeUseCaseTests.ValidCommand(
            primaryIp: "192.168.1.20",
            hostname: "switch-02",
            observedIps: ["192.168.1.10", "192.168.1.20"],
            firstSeenUtc: DateTimeOffset.Parse("2026-04-27T12:00:00Z"),
            lastSeenUtc: DateTimeOffset.Parse("2026-04-27T12:15:00Z")), CancellationToken.None);

        Assert.Equal(DeviceIntakeOutcomeKind.Updated, updated.Kind);
        Assert.Equal(DateTimeOffset.Parse("2026-04-27T12:00:00Z"), updated.Device!.FirstSeenUtc);
        Assert.Equal(DateTimeOffset.Parse("2026-04-27T12:15:00Z"), updated.Device.LastSeenUtc);
        Assert.Equal(["192.168.1.10", "192.168.1.20"], updated.Device.ObservedIps);
        Assert.Equal("switch-02", updated.Device.Hostname);
        Assert.Equal("192.168.1.20", updated.Device.PrimaryIp);
    }

    [Fact]
    public async Task Execute_keeps_existing_non_null_hostname_and_primary_ip_on_timestamp_tie()
    {
        var repository = new Support.InMemoryDeviceInventoryRepository();
        var useCase = CreateUseCase(repository);

        await useCase.Execute(AcceptDeviceIntakeUseCaseTests.ValidCommand(
            primaryIp: "192.168.1.10",
            hostname: "switch-01"), CancellationToken.None);

        var tied = await useCase.Execute(AcceptDeviceIntakeUseCaseTests.ValidCommand(
            primaryIp: "192.168.1.20",
            hostname: "switch-02"), CancellationToken.None);

        Assert.Equal("switch-01", tied.Device!.Hostname);
        Assert.Equal("192.168.1.10", tied.Device.PrimaryIp);
    }

    private static NetworkMonitoring.Backend.Application.UseCases.AcceptDeviceIntakeUseCase CreateUseCase(
        Support.InMemoryDeviceInventoryRepository repository)
    {
        return new NetworkMonitoring.Backend.Application.UseCases.AcceptDeviceIntakeUseCase(
            repository,
            repository,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<NetworkMonitoring.Backend.Application.UseCases.AcceptDeviceIntakeUseCase>.Instance);
    }
}
