using Microsoft.Extensions.Logging.Abstractions;
using NetworkMonitoring.Backend.Application.Models;
using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Backend.UnitTests.Support;

namespace NetworkMonitoring.Backend.UnitTests.Application.UseCases;

public sealed class DeviceIntakeDiagnosticsTests
{
    [Fact]
    public async Task Execute_classifies_created_updated_idempotent_rejected_and_persistence_failure_outcomes()
    {
        var repository = new InMemoryDeviceInventoryRepository();
        var useCase = CreateUseCase(repository);

        var created = await useCase.Execute(AcceptDeviceIntakeUseCaseTests.ValidCommand(), CancellationToken.None);
        var idempotent = await useCase.Execute(AcceptDeviceIntakeUseCaseTests.ValidCommand(), CancellationToken.None);
        var updated = await useCase.Execute(AcceptDeviceIntakeUseCaseTests.ValidCommand(
            observedIps: ["192.168.1.10", "192.168.1.11"],
            lastSeenUtc: DateTimeOffset.Parse("2026-04-27T12:10:00Z")), CancellationToken.None);
        var rejected = await useCase.Execute(AcceptDeviceIntakeUseCaseTests.ValidCommand(macAddress: "not-a-mac"), CancellationToken.None);

        var failingRepository = new InMemoryDeviceInventoryRepository { ThrowOnSave = true };
        var persistenceFailure = await CreateUseCase(failingRepository)
            .Execute(AcceptDeviceIntakeUseCaseTests.ValidCommand(), CancellationToken.None);

        Assert.Equal(DeviceIntakeOutcomeKind.Created, created.Kind);
        Assert.Equal(DeviceIntakeOutcomeKind.Idempotent, idempotent.Kind);
        Assert.Equal(DeviceIntakeOutcomeKind.Updated, updated.Kind);
        Assert.Equal(DeviceIntakeOutcomeKind.Rejected, rejected.Kind);
        Assert.Equal(DeviceIntakeOutcomeKind.PersistenceFailure, persistenceFailure.Kind);
    }

    private static AcceptDeviceIntakeUseCase CreateUseCase(InMemoryDeviceInventoryRepository repository)
    {
        return new AcceptDeviceIntakeUseCase(
            repository,
            repository,
            NullLogger<AcceptDeviceIntakeUseCase>.Instance);
    }
}
