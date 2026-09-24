using Microsoft.Extensions.Logging;
using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Domain.ValueObjects;
using NetworkMonitoring.IntegrationConsole.Application.Models;
using NetworkMonitoring.IntegrationConsole.Application.Ports;

namespace NetworkMonitoring.IntegrationConsole.Application.UseCases;

/// <summary>
/// Use case for processing device detections from a message broker and forwarding them to the backend.
/// </summary>
/// <param name="consumer">The device event consumer.</param>
/// <param name="intakeClient">The backend intake client.</param>
/// <param name="flowTelemetry">Ingestion throughput and lag telemetry.</param>
/// <param name="logger">The logger.</param>
public sealed class ProcessDeviceDetectionsUseCase(
    IDeviceEventConsumer consumer,
    IDeviceIntakeClient intakeClient,
    IIngestionFlowTelemetry flowTelemetry,
    ILogger<ProcessDeviceDetectionsUseCase> logger)
{
    /// <summary>
    /// Runs the detection processing loop.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Run(CancellationToken cancellationToken)
    {
        await foreach (var consumedEvent in consumer.Consume(cancellationToken))
        {
            await Process(consumedEvent, cancellationToken);
        }
    }

    /// <summary>
    /// Processes a single consumed event.
    /// </summary>
    /// <param name="consumedEvent">The event to process.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The outcome of the processing.</returns>
    public async Task<IngestionOutcome> Process(ConsumedDeviceEvent consumedEvent, CancellationToken cancellationToken)
    {
        // First step is to validate the raw event and map it to our internal domain models.
        // This ensures we only send clean data to the backend.
        if (!TryValidate(consumedEvent, out var detectedEvent, out var rejectionReason))
        {
            logger.LogWarning(
                "Rejected DeviceDetected event at {Topic}[{Partition}]@{Offset}: {Reason}",
                consumedEvent.Topic,
                consumedEvent.Partition,
                consumedEvent.Offset,
                rejectionReason);

            // Even for rejected events, we acknowledge them so they are not re-processed.
            // Malformed data is considered "poison" and should be handled (e.g. sent to DLQ) or discarded.
            await consumer.Acknowledge(consumedEvent, cancellationToken);
            flowTelemetry.TrackIngestion("rejected");
            return IngestionOutcome.Rejected(rejectionReason);
        }

        logger.LogInformation(
            "Consumed DeviceDetected for {MacAddress} from {Topic}[{Partition}]@{Offset}",
            detectedEvent.MacAddress,
            consumedEvent.Topic,
            consumedEvent.Partition,
            consumedEvent.Offset);

        // Forwarding to backend. The client handles its own retry logic.
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

        // Successfully processed (or permanently failed) events are acknowledged.
        await consumer.Acknowledge(consumedEvent, cancellationToken);
        flowTelemetry.TrackIngestion(outcome.Kind.ToString().ToLowerInvariant());
        return outcome;
    }

    /// <summary>
    /// Validates a consumed event and converts it to a validated <see cref="DeviceDetectedEvent"/>.
    /// </summary>
    /// <param name="consumedEvent">The consumed event.</param>
    /// <param name="detectedEvent">The resulting validated event.</param>
    /// <param name="rejectionReason">The reason for rejection, if any.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool TryValidate(
        ConsumedDeviceEvent consumedEvent,
        out DeviceDetectedEvent detectedEvent,
        out string rejectionReason)
    {
        detectedEvent = null!;

        // Check if the event was already flagged as malformed during consumption (e.g. Avro deserialization failed).
        if (consumedEvent.IsMalformed)
        {
            rejectionReason = consumedEvent.RejectionReason ?? "Malformed event payload";
            return false;
        }

        // The Kafka message key is expected to be the MAC address.
        if (string.IsNullOrWhiteSpace(consumedEvent.Key))
        {
            rejectionReason = "DeviceDetected key is required";
            return false;
        }

        // Validate MAC address format in both payload and key.
        if (!MacAddress.TryCreate(consumedEvent.Event!.MacAddress, out var eventMac) || eventMac is null)
        {
            rejectionReason = "DeviceDetected payload MAC is invalid";
            return false;
        }

        // Ensure consistency between Kafka key and message payload.
        if (!MacAddress.TryCreate(consumedEvent.Key, out var keyMac) || keyMac is null || !eventMac.Equals(keyMac))
        {
            rejectionReason = "DeviceDetected key does not match payload MAC";
            return false;
        }

        // Attempt to create a domain Entity to leverage business logic validation.
        if (!TryCreateDomainDevice(consumedEvent.Event, eventMac, out var domainDevice, out rejectionReason))
        {
            return false;
        }

        // Map back to a clean record for transport, using normalized values from the domain objects.
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

        // Validate IP addresses using domain ValueObjects.
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
            // The Device.Create method will throw if domain invariants are violated.
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
