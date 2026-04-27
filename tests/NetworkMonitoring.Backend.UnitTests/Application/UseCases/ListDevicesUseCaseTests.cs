using Microsoft.Extensions.Logging.Abstractions;
using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Backend.UnitTests.Support;

namespace NetworkMonitoring.Backend.UnitTests.Application.UseCases;

public sealed class ListDevicesUseCaseTests
{
    [Fact]
    public async Task Execute_returns_inventory_items_ordered_by_mac()
    {
        var repository = new InMemoryDeviceInventoryRepository();
        var intake = new AcceptDeviceIntakeUseCase(
            repository,
            repository,
            NullLogger<AcceptDeviceIntakeUseCase>.Instance);

        await intake.Execute(AcceptDeviceIntakeUseCaseTests.ValidCommand(
            idempotencyKey: "BB:BB:BB:BB:BB:BB",
            macAddress: "BB:BB:BB:BB:BB:BB"), CancellationToken.None);
        await intake.Execute(AcceptDeviceIntakeUseCaseTests.ValidCommand(), CancellationToken.None);

        var items = await new ListDevicesUseCase(repository).Execute(CancellationToken.None);

        Assert.Equal(["AA:BB:CC:DD:EE:FF", "BB:BB:BB:BB:BB:BB"], items.Select(item => item.MacAddress).ToArray());
    }
}
