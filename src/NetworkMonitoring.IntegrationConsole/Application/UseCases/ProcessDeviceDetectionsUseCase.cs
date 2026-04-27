using Microsoft.Extensions.Logging;
using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Domain.ValueObjects;
using NetworkMonitoring.IntegrationConsole.Application.Models;
using NetworkMonitoring.IntegrationConsole.Application.Ports;

namespace NetworkMonitoring.IntegrationConsole.Application.UseCases;

public sealed class ProcessDeviceDetectionsUseCase(
    IDeviceEventConsumer consumer,
    IDeviceIntakeClient intakeClient,
    ILogger<ProcessDeviceDetectionsUseCase> logger)
{
    public async Task Run(CancellationToken cancellationToken)
    {
        await foreach (var consumedEvent in consumer.Consume(cancellationToken))
        {
            await Process(consumedEvent, cancellationToken);
        }
    }

    public async Task<IngestionOutcome> Process(ConsumedDeviceEvent consumedEvent, CancellationToken cancellationToken)
    {
        if (!TryValidate(consumedEvent, out var detectedEvent, out var rejectionReason))
        {
            logger.LogWarning(
                "Rejected DeviceDetected event at {Topic}[{Partition}]@{Offset}: {Reason}",
                consumedEvent.Topic,
                consumedEvent.Partition,
                consumedEvent.Offset,
                rejectionReason);

            await consumer.Acknowledge(consumedEvent, cancellationToken);
            return IngestionOutcome.Rejected(rejectionReason);
        }

        logger.LogInformation(
            "Consumed DeviceDetected for {MacAddress} from {Topic}[{Partition}]@{Offset}",
            detectedEvent.MacAddress,
            consumedEvent.Topic,
            consumedEvent.Partition,
            consumedEvent.Offset);

        var outcome = await intakeClient.Send(detectedEvent, cancellationToken);

        if (outcome.Kind is IngestionOutcomeKind.Succeeded)
        {
            logger.LogInformation(
                "Forwarded DeviceDetected for {MacAddress} after {AttemptCount} attempt(s)",
                detectedEvent.MacAddress,
                outcome.AttemptCount);
        }
        else
        {
            logger.LogWarning(
                "DeviceDetected forwarding for {MacAddress} ended with {Outcome}: {Reason}",
                detectedEvent.MacAddress,
                outcome.Kind,
                outcome.Reason);
        }

        await consumer.Acknowledge(consumedEvent, cancellationToken);
        return outcome;
    }

    public static bool TryValidate(
        ConsumedDeviceEvent consumedEvent,
        out DeviceDetectedEvent detectedEvent,
        out string rejectionReason)
    {
        detectedEvent = null!;

        if (consumedEvent.IsMalformed)
        {
            rejectionReason = consumedEvent.RejectionReason ?? "Malformed event payload";
            return false;
        }

        if (string.IsNullOrWhiteSpace(consumedEvent.Key))
        {
            rejectionReason = "DeviceDetected key is required";
            return false;
        }

        if (!MacAddress.TryCreate(consumedEvent.Event!.MacAddress, out var eventMac) || eventMac is null)
        {
            rejectionReason = "DeviceDetected payload MAC is invalid";
            return false;
        }

        if (!MacAddress.TryCreate(consumedEvent.Key, out var keyMac) || keyMac is null || !eventMac.Equals(keyMac))
        {
            rejectionReason = "DeviceDetected key does not match payload MAC";
            return false;
        }

        if (!TryCreateDomainDevice(consumedEvent.Event, eventMac, out var domainDevice, out rejectionReason))
        {
            return false;
        }

        detectedEvent = consumedEvent.Event with
        {
            MacAddress = domainDevice.MacAddress.Value,
            PrimaryIp = domainDevice.PrimaryIp?.Value,
            ObservedIps = domainDevice.ObservedIps.Select(ip => ip.Value).ToArray(),
            DiscoverySource = domainDevice.DiscoverySource.Value,
            Hostname = domainDevice.Hostname
        };
        rejectionReason = string.Empty;
        return true;
    }

    private static bool TryCreateDomainDevice(
        DeviceDetectedEvent detectedEvent,
        MacAddress macAddress,
        out Device domainDevice,
        out string rejectionReason)
    {
        domainDevice = null!;

        if (detectedEvent.PrimaryIp is not null && !IpAddress.TryCreate(detectedEvent.PrimaryIp, out _))
        {
            rejectionReason = "DeviceDetected primary IP is invalid";
            return false;
        }

        var primaryIp = detectedEvent.PrimaryIp is null
            ? null
            : new IpAddress(detectedEvent.PrimaryIp);

        var observedIps = new List<IpAddress>();
        foreach (var observedIp in detectedEvent.ObservedIps)
        {
            if (!IpAddress.TryCreate(observedIp, out var ipAddress))
            {
                rejectionReason = "DeviceDetected observed IP is invalid";
                return false;
            }

            observedIps.Add(ipAddress!);
        }

        try
        {
            domainDevice = Device.Create(
                detectedEvent.DeviceId,
                macAddress,
                primaryIp,
                detectedEvent.Hostname,
                observedIps,
                detectedEvent.FirstSeenUtc,
                detectedEvent.LastSeenUtc,
                DiscoverySource.FromRaw(detectedEvent.DiscoverySource));
            rejectionReason = string.Empty;
            return true;
        }
        catch (ArgumentException ex)
        {
            rejectionReason = ex.Message;
            return false;
        }
    }
}
