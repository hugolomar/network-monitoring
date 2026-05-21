using System.Text;
using NetworkMonitoring.Domain.Entities;

namespace NetworkMonitoring.Probe.Infrastructure.Publishing;

/// <summary>
/// Kafka message key for device detections: normalized MAC address from the payload.
/// </summary>
public static class DeviceKafkaPartitionKey
{
    /// <summary>
    /// Builds a deterministic string key for Kafka partitioning based on the device MAC address.
    /// </summary>
    /// <param name="device">The device to build a key for.</param>
    /// <returns>A string representation of the partition key.</returns>
    public static string Build(Device device) => device.MacAddress.Value;

    /// <summary>
    /// Builds a deterministic byte-array key for Kafka partitioning.
    /// </summary>
    /// <param name="device">The device to build a key for.</param>
    /// <returns>The key encoded as UTF-8 bytes.</returns>
    public static byte[] BuildUtf8Bytes(Device device) => Encoding.UTF8.GetBytes(Build(device));
}
