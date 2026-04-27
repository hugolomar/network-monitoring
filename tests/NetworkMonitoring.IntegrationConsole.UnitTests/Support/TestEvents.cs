using NetworkMonitoring.IntegrationConsole.Application.Models;

namespace NetworkMonitoring.IntegrationConsole.UnitTests.Support;

internal static class TestEvents
{
    public static DeviceDetectedEvent DeviceDetected(string macAddress = "AA:BB:CC:DD:EE:FF") =>
        new(
            "DeviceDetected",
            DateTimeOffset.Parse("2026-04-27T12:05:01.0000000+00:00"),
            "probe",
            1,
            null,
            macAddress,
            "192.168.1.10",
            "switch-01",
            ["192.168.1.10", "192.168.1.11"],
            DateTimeOffset.Parse("2026-04-27T12:00:00.0000000+00:00"),
            DateTimeOffset.Parse("2026-04-27T12:05:00.0000000+00:00"),
            "TRAFFIC");

    public static ConsumedDeviceEvent Consumed(DeviceDetectedEvent? detectedEvent = null, string? key = "AA:BB:CC:DD:EE:FF") =>
        new(key, detectedEvent ?? DeviceDetected(), "devices.detected", 0, 42);
}
