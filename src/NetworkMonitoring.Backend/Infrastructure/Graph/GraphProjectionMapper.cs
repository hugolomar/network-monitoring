namespace NetworkMonitoring.Backend.Infrastructure.Graph;

/// <summary>
/// Resolves projection destination identity/category semantics.
/// </summary>
public static class GraphProjectionMapper
{
    /// <summary>
    /// Resolves destination identity and kind from internal and external evidence.
    /// </summary>
    public static (string DestinationIdentity, string DestinationKind) ResolveDestination(
        string? destinationDeviceId,
        string? destinationIp)
    {
        if (!string.IsNullOrWhiteSpace(destinationDeviceId))
        {
            return (destinationDeviceId, "InternalDevice");
        }

        if (!string.IsNullOrWhiteSpace(destinationIp))
        {
            return (destinationIp, "ExternalHost");
        }

        throw new ArgumentException("Either destinationDeviceId or destinationIp must be provided.");
    }
}
