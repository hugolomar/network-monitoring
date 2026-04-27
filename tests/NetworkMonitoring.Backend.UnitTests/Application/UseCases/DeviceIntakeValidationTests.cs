using Microsoft.Extensions.Logging.Abstractions;
using NetworkMonitoring.Backend.Application.Models;
using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Backend.UnitTests.Support;

namespace NetworkMonitoring.Backend.UnitTests.Application.UseCases;

public sealed class DeviceIntakeValidationTests
{
    [Fact]
    public async Task Execute_rejects_missing_idempotency_key()
    {
        var outcome = await CreateUseCase().Execute(
            AcceptDeviceIntakeUseCaseTests.ValidCommand(idempotencyKey: ""),
            CancellationToken.None);

        Assert.Equal(DeviceIntakeOutcomeKind.Rejected, outcome.Kind);
    }

    [Fact]
    public async Task Execute_rejects_mac_identity_mismatch()
    {
        var outcome = await CreateUseCase().Execute(
            AcceptDeviceIntakeUseCaseTests.ValidCommand(idempotencyKey: "11:22:33:44:55:66"),
            CancellationToken.None);

        Assert.Equal(DeviceIntakeOutcomeKind.Rejected, outcome.Kind);
    }

    [Fact]
    public async Task Execute_rejects_invalid_ip_values()
    {
        var outcome = await CreateUseCase().Execute(
            AcceptDeviceIntakeUseCaseTests.ValidCommand(observedIps: ["not-an-ip"]),
            CancellationToken.None);

        Assert.Equal(DeviceIntakeOutcomeKind.Rejected, outcome.Kind);
    }

    [Fact]
    public async Task Execute_rejects_missing_required_timestamps()
    {
        var command = new DeviceIntakeCommand(
            "AA:BB:CC:DD:EE:FF",
            "AA:BB:CC:DD:EE:FF",
            "192.168.1.10",
            "switch-01",
            ["192.168.1.10"],
            null,
            DateTimeOffset.Parse("2026-04-27T12:05:00Z"),
            "TRAFFIC",
            null);

        var outcome = await CreateUseCase().Execute(command, CancellationToken.None);

        Assert.Equal(DeviceIntakeOutcomeKind.Rejected, outcome.Kind);
        Assert.Contains("firstSeenUtc", outcome.Reason);
    }

    [Fact]
    public async Task Execute_rejects_invalid_timestamp_ordering()
    {
        var outcome = await CreateUseCase().Execute(
            AcceptDeviceIntakeUseCaseTests.ValidCommand(
                firstSeenUtc: DateTimeOffset.Parse("2026-04-27T12:05:00Z"),
                lastSeenUtc: DateTimeOffset.Parse("2026-04-27T12:00:00Z")),
            CancellationToken.None);

        Assert.Equal(DeviceIntakeOutcomeKind.Rejected, outcome.Kind);
    }

    private static AcceptDeviceIntakeUseCase CreateUseCase()
    {
        var repository = new InMemoryDeviceInventoryRepository();
        return new AcceptDeviceIntakeUseCase(
            repository,
            repository,
            NullLogger<AcceptDeviceIntakeUseCase>.Instance);
    }
}
