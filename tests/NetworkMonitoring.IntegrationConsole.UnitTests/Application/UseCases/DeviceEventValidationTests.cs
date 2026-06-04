using NetworkMonitoring.IntegrationConsole.Application.UseCases;
using NetworkMonitoring.IntegrationConsole.UnitTests.Support;

namespace NetworkMonitoring.IntegrationConsole.UnitTests.Application.UseCases;

/// <summary>
/// Test suite for DeviceEventValidation.
/// </summary>
public sealed class DeviceEventValidationTests
{
    /// <summary>
    /// Verifies that try validate accepts key that normalizes to payload mac.
    /// </summary>
    [Theory]
    [InlineData("aa-bb-cc-dd-ee-ff")]
    [InlineData("AABBCCDDEEFF")]
    public void TryValidate_accepts_key_that_normalizes_to_payload_mac(string key)
    {
        var valid = ProcessDeviceDetectionsUseCase.TryValidate(TestEvents.Consumed(key: key), out var detectedEvent, out _);

        Assert.True(valid);
        Assert.Equal("AA:BB:CC:DD:EE:FF", detectedEvent.MacAddress);
    }

    /// <summary>
    /// Verifies that try validate rejects key payload identity mismatch.
    /// </summary>
    [Fact]
    public void TryValidate_rejects_key_payload_identity_mismatch()
    {
        var valid = ProcessDeviceDetectionsUseCase.TryValidate(TestEvents.Consumed(key: "11:22:33:44:55:66"), out _, out var reason);

        Assert.False(valid);
        Assert.Contains("key does not match", reason);
    }

    /// <summary>
    /// Verifies that try validate rejects missing key.
    /// </summary>
    [Fact]
    public void TryValidate_rejects_missing_key()
    {
        var valid = ProcessDeviceDetectionsUseCase.TryValidate(TestEvents.Consumed(key: null), out _, out var reason);

        Assert.False(valid);
        Assert.Contains("key is required", reason);
    }
}
