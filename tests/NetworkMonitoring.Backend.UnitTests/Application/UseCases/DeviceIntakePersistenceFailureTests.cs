using Microsoft.Extensions.Logging.Abstractions;
using NetworkMonitoring.Backend.Application.Models;
using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Backend.UnitTests.Support;

namespace NetworkMonitoring.Backend.UnitTests.Application.UseCases;

public sealed class DeviceIntakePersistenceFailureTests
{
    [Fact]
    public async Task Execute_classifies_persistence_failure_as_service_unavailable()
    {
        var repository = new InMemoryDeviceInventoryRepository { ThrowOnSave = true };
        var useCase = new AcceptDeviceIntakeUseCase(
            repository,
            repository,
            NullLogger<AcceptDeviceIntakeUseCase>.Instance);

        var outcome = await useCase.Execute(AcceptDeviceIntakeUseCaseTests.ValidCommand(), CancellationToken.None);

        Assert.Equal(DeviceIntakeOutcomeKind.PersistenceFailure, outcome.Kind);
        Assert.Equal(503, outcome.StatusCode);
    }
}
