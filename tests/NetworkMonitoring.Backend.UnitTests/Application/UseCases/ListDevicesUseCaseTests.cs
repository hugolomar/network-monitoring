using Microsoft.Extensions.Logging.Abstractions;
using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Backend.UnitTests.Support;

namespace NetworkMonitoring.Backend.UnitTests.Application.UseCases;

/// <summary>
/// Verifies the behavior of the ListDevicesUseCase, which is responsible for 
/// retrieving the inventory of managed devices from the system.
/// </summary>
public sealed class ListDevicesUseCaseTests
{
    /// <summary>
    /// Verifies that executing the use case returns all existing device inventory items 
    /// and that the results are correctly ordered by their MAC address.
    /// </summary>
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
