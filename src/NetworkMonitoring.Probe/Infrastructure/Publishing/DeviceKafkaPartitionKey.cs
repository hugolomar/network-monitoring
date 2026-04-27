using System.Text;
using NetworkMonitoring.Domain.Entities;

namespace NetworkMonitoring.Probe.Infrastructure.Publishing;

/// <summary>
/// Kafka message key for device detections: normalized MAC address from the payload.
/// </summary>
public static class DeviceKafkaPartitionKey
{
    public static string Build(Device device) => device.MacAddress.Value;

    public static byte[] BuildUtf8Bytes(Device device) => Encoding.UTF8.GetBytes(Build(device));
}
