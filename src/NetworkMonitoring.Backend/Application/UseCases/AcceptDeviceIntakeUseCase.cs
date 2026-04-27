using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using NetworkMonitoring.Backend.Application.Models;
using NetworkMonitoring.Backend.Application.Ports;
using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Domain.ValueObjects;

namespace NetworkMonitoring.Backend.Application.UseCases;

public sealed class AcceptDeviceIntakeUseCase(
    IDeviceInventoryRepository repository,
    IInventoryUnitOfWork unitOfWork,
    ILogger<AcceptDeviceIntakeUseCase> logger)
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> MacLocks = new(StringComparer.Ordinal);

    public async Task<DeviceIntakeOutcome> Execute(DeviceIntakeCommand command, CancellationToken cancellationToken)
    {
        if (!TryBuildIncomingDevice(command, out var incoming, out var rejectionReason))
        {
            logger.LogWarning("Rejected device intake: {Reason}", rejectionReason);
            return DeviceIntakeOutcome.Rejected(rejectionReason);
        }

        var macLock = MacLocks.GetOrAdd(incoming.MacAddress.Value, _ => new SemaphoreSlim(1, 1));
        await macLock.WaitAsync(cancellationToken);

        try
        {
            var existing = await repository.GetByMacAddress(incoming.MacAddress.Value, cancellationToken);
            if (existing is null)
            {
                await repository.Add(incoming, cancellationToken);
                await unitOfWork.SaveChanges(cancellationToken);

                logger.LogInformation("Accepted new device intake for {MacAddress}", incoming.MacAddress.Value);
                return DeviceIntakeOutcome.Created(ToItem(incoming));
            }

            var consolidated = Consolidate(existing, incoming);
            await repository.Update(consolidated.Device, cancellationToken);
            await unitOfWork.SaveChanges(cancellationToken);

            if (!consolidated.Changed)
            {
                logger.LogInformation("Accepted idempotent duplicate device intake for {MacAddress}", existing.MacAddress.Value);
                return DeviceIntakeOutcome.Idempotent(ToItem(consolidated.Device));
            }

            logger.LogInformation("Updated device inventory state for {MacAddress}", existing.MacAddress.Value);
            return DeviceIntakeOutcome.Updated(ToItem(consolidated.Device));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Persistence failure while accepting device intake for {MacAddress}", incoming.MacAddress.Value);
            return DeviceIntakeOutcome.PersistenceFailure("Device inventory persistence dependency is unavailable.");
        }
        finally
        {
            macLock.Release();
        }
    }

    private static bool TryBuildIncomingDevice(
        DeviceIntakeCommand command,
        out Device device,
        out string rejectionReason)
    {
        device = null!;

        if (!MacAddress.TryCreate(command.IdempotencyKey, out var idempotencyMac) || idempotencyMac is null)
        {
            rejectionReason = "Idempotency-Key is required and must be a valid MAC address.";
            return false;
        }

        if (!MacAddress.TryCreate(command.MacAddress, out var bodyMac) || bodyMac is null)
        {
            rejectionReason = "macAddress is required and must be a valid MAC address.";
            return false;
        }

        if (!idempotencyMac.Equals(bodyMac))
        {
            rejectionReason = "Idempotency-Key must match body macAddress.";
            return false;
        }

        if (command.FirstSeenUtc is null)
        {
            rejectionReason = "firstSeenUtc is required.";
            return false;
        }

        if (command.LastSeenUtc is null)
        {
            rejectionReason = "lastSeenUtc is required.";
            return false;
        }

        if (command.LastSeenUtc < command.FirstSeenUtc)
        {
            rejectionReason = "lastSeenUtc must be greater than or equal to firstSeenUtc.";
            return false;
        }

        if (command.PrimaryIp is not null && !IpAddress.TryCreate(command.PrimaryIp, out _))
        {
            rejectionReason = "primaryIp must be a valid IP address.";
            return false;
        }

        var primaryIp = command.PrimaryIp is null ? null : new IpAddress(command.PrimaryIp);
        var observedIps = new List<IpAddress>();
        foreach (var observedIp in command.ObservedIps ?? Array.Empty<string>())
        {
            if (!IpAddress.TryCreate(observedIp, out var ipAddress) || ipAddress is null)
            {
                rejectionReason = "observedIps must contain only valid IP addresses.";
                return false;
            }

            observedIps.Add(ipAddress);
        }

        try
        {
            device = Device.Create(
                null,
                bodyMac,
                primaryIp,
                command.Hostname,
                observedIps,
                command.FirstSeenUtc.Value,
                command.LastSeenUtc.Value,
                DiscoverySource.FromRaw(command.DiscoverySource));
            rejectionReason = string.Empty;
            return true;
        }
        catch (ArgumentException ex)
        {
            rejectionReason = ex.Message;
            return false;
        }
    }

    private static (Device Device, bool Changed) Consolidate(Device existing, Device incoming)
    {
        var observedIps = existing.ObservedIps.Union(incoming.ObservedIps).ToArray();
        var firstSeenUtc = existing.FirstSeenUtc <= incoming.FirstSeenUtc ? existing.FirstSeenUtc : incoming.FirstSeenUtc;
        var lastSeenUtc = existing.LastSeenUtc >= incoming.LastSeenUtc ? existing.LastSeenUtc : incoming.LastSeenUtc;
        var hostname = SelectLatestKnown(existing.Hostname, incoming.Hostname, existing.LastSeenUtc, incoming.LastSeenUtc);
        var primaryIp = SelectLatestKnown(existing.PrimaryIp, incoming.PrimaryIp, existing.LastSeenUtc, incoming.LastSeenUtc);
        var discoverySource = incoming.LastSeenUtc > existing.LastSeenUtc ? incoming.DiscoverySource : existing.DiscoverySource;

        var consolidated = Device.Create(
            existing.Id,
            existing.MacAddress,
            primaryIp,
            hostname,
            observedIps,
            firstSeenUtc,
            lastSeenUtc,
            discoverySource);

        return (consolidated, HasChanged(existing, consolidated));
    }

    private static T? SelectLatestKnown<T>(T? stored, T? incoming, DateTimeOffset storedLastSeen, DateTimeOffset incomingLastSeen)
    {
        if (incoming is null)
        {
            return stored;
        }

        if (stored is null)
        {
            return incoming;
        }

        return incomingLastSeen > storedLastSeen ? incoming : stored;
    }

    private static bool HasChanged(Device existing, Device consolidated)
    {
        return existing.FirstSeenUtc != consolidated.FirstSeenUtc
            || existing.LastSeenUtc != consolidated.LastSeenUtc
            || existing.Hostname != consolidated.Hostname
            || existing.PrimaryIp?.Value != consolidated.PrimaryIp?.Value
            || existing.DiscoverySource.Value != consolidated.DiscoverySource.Value
            || !existing.ObservedIps.Select(ip => ip.Value).Order().SequenceEqual(
                consolidated.ObservedIps.Select(ip => ip.Value).Order());
    }

    internal static DeviceInventoryItem ToItem(Device device)
    {
        return new DeviceInventoryItem(
            device.Id ?? 0,
            device.MacAddress.Value,
            device.PrimaryIp?.Value,
            device.Hostname,
            device.ObservedIps.Select(ip => ip.Value).Order().ToArray(),
            device.FirstSeenUtc,
            device.LastSeenUtc,
            device.DiscoverySource.Value);
    }
}
