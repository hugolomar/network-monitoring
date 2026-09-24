using Microsoft.Extensions.Logging.Abstractions;
using NetworkMonitoring.Backend.Application.Models;
using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Backend.UnitTests.Support;

namespace NetworkMonitoring.Backend.UnitTests.Application.UseCases;

/// <summary>
/// Verifies the behavior of the AcceptDeviceIntakeUseCase, which is responsible 
/// for processing, validating, and accepting incoming device telemetry data.
/// </summary>
public sealed class AcceptDeviceIntakeUseCaseTests
{
    /// <summary>
    /// Verifies that a valid intake command is successfully accepted, using the 
    /// shared domain logic to correctly normalize fields like the MAC address.
    /// </summary>
    [Fact]
    public async Task Execute_accepts_valid_intake_using_shared_domain_normalization()
    {
        var useCase = CreateUseCase();

        var outcome = await useCase.Execute(ValidCommand(macAddress: "aa-bb-cc-dd-ee-ff"), CancellationToken.None);

        Assert.Equal(DeviceIntakeOutcomeKind.Created, outcome.Kind);
        Assert.Equal("AA:BB:CC:DD:EE:FF", outcome.Device!.MacAddress);
        Assert.Equal("192.168.1.10", outcome.Device.PrimaryIp);
    }

    /// <summary>
    /// Verifies that an intake command is rejected if the provided Idempotency-Key 
    /// does not match the normalized MAC address of the payload.
    /// </summary>
    [Fact]
    public async Task Execute_rejects_mismatched_idempotency_key()
    {
        var useCase = CreateUseCase();

        var outcome = await useCase.Execute(ValidCommand(idempotencyKey: "11:22:33:44:55:66"), CancellationToken.None);

        Assert.Equal(DeviceIntakeOutcomeKind.Rejected, outcome.Kind);
        Assert.Contains("Idempotency-Key", outcome.Reason);
    }

    /// <summary>
    /// Verifies that an intake command is rejected if the primary IP address 
    /// provided in the evidence is not a valid IP address format.
    /// </summary>
    [Fact]
    public async Task Execute_rejects_invalid_ip_evidence()
    {
        var useCase = CreateUseCase();

        var outcome = await useCase.Execute(ValidCommand(primaryIp: "not-an-ip"), CancellationToken.None);

        Assert.Equal(DeviceIntakeOutcomeKind.Rejected, outcome.Kind);
        Assert.Contains("primaryIp", outcome.Reason);
    }

    /// <summary>
    /// Verifies that an intake command is rejected if the chronological ordering 
    /// of the first seen and last seen timestamps is invalid (e.g., first seen is after last seen).
    /// </summary>
    [Fact]
    public async Task Execute_rejects_invalid_timestamp_ordering()
    {
        var useCase = CreateUseCase();

        var outcome = await useCase.Execute(ValidCommand(
            firstSeenUtc: DateTimeOffset.Parse("2026-04-27T12:05:00Z"),
            lastSeenUtc: DateTimeOffset.Parse("2026-04-27T12:00:00Z")), CancellationToken.None);

        Assert.Equal(DeviceIntakeOutcomeKind.Rejected, outcome.Kind);
        Assert.Contains("lastSeenUtc", outcome.Reason);
    }

    internal static DeviceIntakeCommand ValidCommand(
        string idempotencyKey = "AA:BB:CC:DD:EE:FF",
        string macAddress = "AA:BB:CC:DD:EE:FF",
        string? primaryIp = "192.168.1.10",
        string? hostname = "switch-01",
        string[]? observedIps = null,
        DateTimeOffset? firstSeenUtc = null,
        DateTimeOffset? lastSeenUtc = null)
    {
        return new DeviceIntakeCommand(
            idempotencyKey,
            macAddress,
            primaryIp,
            hostname,
            observedIps ?? ["192.168.1.10"],
            firstSeenUtc ?? DateTimeOffset.Parse("2026-04-27T12:00:00Z"),
            lastSeenUtc ?? DateTimeOffset.Parse("2026-04-27T12:05:00Z"),
            "TRAFFIC",
            new SourceEventMetadata("DeviceDetected", "probe", 1, DateTimeOffset.Parse("2026-04-27T12:05:01Z")));
    }

    private static AcceptDeviceIntakeUseCase CreateUseCase(InMemoryDeviceInventoryRepository? repository = null)
    {
        repository ??= new InMemoryDeviceInventoryRepository();
        return new AcceptDeviceIntakeUseCase(
            repository,
            repository,
            NoOpIntakeFlowTelemetry.Instance,
            NullLogger<AcceptDeviceIntakeUseCase>.Instance);
    }
}
