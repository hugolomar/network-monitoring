using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Domain.ValueObjects;
using NetworkMonitoring.Probe.Application.Configuration;
using NetworkMonitoring.Probe.Application.Models;
using NetworkMonitoring.Probe.Application.Ports;

namespace NetworkMonitoring.Probe.Application.UseCases;

public sealed class ProcessObservationsUseCase(
    ITrafficProvider trafficProvider,
    IMessagePublisher messagePublisher,
    IOptions<ProbeOptions> options,
    ILogger<ProcessObservationsUseCase> logger)
{
    private readonly Dictionary<SessionFingerprint, DateTimeOffset> _recentSessions = new();
    private readonly Dictionary<string, DateTimeOffset> _recentDeviceEmissions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Device> _knownDevices = new(StringComparer.OrdinalIgnoreCase);
    private readonly TimeSpan _sessionDeduplicationWindow = ResolveSessionDeduplicationWindow(options.Value);
    private readonly TimeSpan _deviceDeduplicationWindow = ResolveDeviceDeduplicationWindow(options.Value);

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await foreach (var observation in trafficProvider.ReadObservations(cancellationToken))
        {
            var validation = ValidateObservation(observation, out var sourceIp, out var destinationIp);
            if (!validation.IsValid || sourceIp is null || destinationIp is null)
            {
                logger.LogWarning(
                    "Skipping invalid observation: {SourceIp} -> {DestinationIp}. Errors: {Errors}",
                    observation.SourceIp,
                    observation.DestinationIp,
                    string.Join("; ", validation.Errors));
                continue;
            }

            try
            {
                var session = BuildSession(observation, sourceIp, destinationIp);
                if (ShouldPublishSession(session, observation.ObservedAtUtc))
                {
                    await messagePublisher.PublishSessionDetected(session, cancellationToken);
                }
                else
                {
                    logger.LogDebug(
                        "Skipping duplicated session in deduplication window: {SourceIp}:{SourcePort} -> {DestinationIp}:{DestinationPort} ({Protocol})",
                        session.SourceIp.Value,
                        session.SourcePort?.Value,
                        session.DestinationIp.Value,
                        session.DestinationPort?.Value,
                        session.Protocol.Value);
                }

                await ProcessDeviceDetection(observation, observation.SourceMac, sourceIp, cancellationToken);
                await ProcessDeviceDetection(observation, observation.DestinationMac, destinationIp, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unexpected error while processing observation: {SourceIp} -> {DestinationIp}",
                    observation.SourceIp,
                    observation.DestinationIp);
            }
        }
    }

    private bool ShouldPublishSession(Session session, DateTimeOffset observedAtUtc)
    {
        if (_sessionDeduplicationWindow <= TimeSpan.Zero)
        {
            return true;
        }

        PruneExpiredSessionEntries(observedAtUtc);

        var fingerprint = SessionFingerprint.From(session);
        if (_recentSessions.TryGetValue(fingerprint, out var lastSeenUtc)
            && observedAtUtc - lastSeenUtc < _sessionDeduplicationWindow)
        {
            return false;
        }

        _recentSessions[fingerprint] = observedAtUtc;
        return true;
    }

    private void PruneExpiredSessionEntries(DateTimeOffset nowUtc)
    {
        if (_recentSessions.Count == 0)
        {
            return;
        }

        var keysToRemove = _recentSessions
            .Where(entry => nowUtc - entry.Value >= _sessionDeduplicationWindow)
            .Select(entry => entry.Key)
            .ToArray();

        foreach (var key in keysToRemove)
        {
            _recentSessions.Remove(key);
        }
    }

    private bool ShouldPublishDevice(string normalizedMac, DateTimeOffset observedAtUtc)
    {
        if (_deviceDeduplicationWindow <= TimeSpan.Zero)
        {
            return true;
        }

        PruneExpiredDeviceEmissionEntries(observedAtUtc);

        if (_recentDeviceEmissions.TryGetValue(normalizedMac, out var lastEmittedUtc)
            && observedAtUtc - lastEmittedUtc < _deviceDeduplicationWindow)
        {
            return false;
        }

        _recentDeviceEmissions[normalizedMac] = observedAtUtc;
        return true;
    }

    private void PruneExpiredDeviceEmissionEntries(DateTimeOffset nowUtc)
    {
        if (_recentDeviceEmissions.Count == 0)
        {
            return;
        }

        var keysToRemove = _recentDeviceEmissions
            .Where(entry => nowUtc - entry.Value >= _deviceDeduplicationWindow)
            .Select(entry => entry.Key)
            .ToArray();

        foreach (var key in keysToRemove)
        {
            _recentDeviceEmissions.Remove(key);
        }
    }

    private static TimeSpan ResolveSessionDeduplicationWindow(ProbeOptions options)
    {
        if (options.SessionDeduplicationWindowMinutes <= 0)
        {
            return TimeSpan.Zero;
        }

        return TimeSpan.FromMinutes(options.SessionDeduplicationWindowMinutes);
    }

    private static TimeSpan ResolveDeviceDeduplicationWindow(ProbeOptions options)
    {
        if (options.DeviceDeduplicationWindowMinutes <= 0)
        {
            return TimeSpan.Zero;
        }

        return TimeSpan.FromMinutes(options.DeviceDeduplicationWindowMinutes);
    }

    private static ObservationValidationResult ValidateObservation(
        TrafficObservation observation,
        out IpAddress? sourceIp,
        out IpAddress? destinationIp)
    {
        sourceIp = null;
        destinationIp = null;
        var errors = new List<string>();

        if (!IpAddress.TryCreate(observation.SourceIp, out sourceIp) || sourceIp is null)
        {
            errors.Add("Invalid source IP address.");
        }

        if (!IpAddress.TryCreate(observation.DestinationIp, out destinationIp) || destinationIp is null)
        {
            errors.Add("Invalid destination IP address.");
        }

        if (observation.BytesObserved < 0)
        {
            errors.Add("BytesObserved cannot be negative.");
        }

        return errors.Count == 0
            ? ObservationValidationResult.Valid()
            : ObservationValidationResult.Invalid([.. errors]);
    }

    private static Session BuildSession(TrafficObservation observation, IpAddress sourceIp, IpAddress destinationIp)
    {
        Port.TryCreate(observation.SourcePort, out var sourcePort);
        Port.TryCreate(observation.DestinationPort, out var destinationPort);

        return Session.Create(
            null,
            sourceIp,
            destinationIp,
            sourcePort,
            destinationPort,
            ProtocolType.FromRaw(observation.Protocol),
            observation.ObservedAtUtc,
            observation.ObservedAtUtc,
            observation.BytesObserved);
    }

    private async Task ProcessDeviceDetection(
        TrafficObservation observation,
        string? mac,
        IpAddress ip,
        CancellationToken cancellationToken)
    {
        var discoveryValidation = ValidateDiscovery(mac);
        if (!discoveryValidation.IsValid)
        {
            logger.LogWarning(
                "Skipping invalid discovery evidence for observation {SourceIp} -> {DestinationIp}. Errors: {Errors}",
                observation.SourceIp,
                observation.DestinationIp,
                string.Join("; ", discoveryValidation.Errors));
            return;
        }

        if (!MacAddress.TryCreate(mac, out var macAddress) || macAddress is null)
        {
            return;
        }

        var source = DiscoverySource.FromRaw(observation.DiscoverySource);
        var device = BuildDevice(observation, macAddress, ip, source);

        if (_knownDevices.TryGetValue(macAddress.Value, out var existingDevice))
        {
            existingDevice.ConsolidateDetection(ip, observation.Hostname, observation.ObservedAtUtc, source);
            device = existingDevice;
        }
        else
        {
            _knownDevices[macAddress.Value] = device;
        }

        if (!ShouldPublishDevice(macAddress.Value, observation.ObservedAtUtc))
        {
            logger.LogDebug(
                "Skipping duplicated DeviceDetected in deduplication window: {Mac}",
                macAddress.Value);
            return;
        }

        await messagePublisher.PublishDeviceDetected(device, cancellationToken);
    }

    private static DiscoveryValidationResult ValidateDiscovery(string? mac)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(mac))
        {
            errors.Add("MAC evidence is required for discovery emission.");
            return DiscoveryValidationResult.Invalid([.. errors]);
        }

        if (!MacAddress.TryCreate(mac, out _))
        {
            errors.Add("MAC address format is invalid.");
            return DiscoveryValidationResult.Invalid([.. errors]);
        }

        return DiscoveryValidationResult.Valid();
    }

    private static Device BuildDevice(
        TrafficObservation observation,
        MacAddress macAddress,
        IpAddress ip,
        DiscoverySource source) =>
        Device.Create(
            null,
            macAddress,
            ip,
            observation.Hostname,
            [ip],
            observation.ObservedAtUtc,
            observation.ObservedAtUtc,
            source);

    private readonly record struct SessionFingerprint(
        string SourceIp,
        string DestinationIp,
        int? SourcePort,
        int? DestinationPort,
        string Protocol)
    {
        public static SessionFingerprint From(Session session) =>
            new(
                session.SourceIp.Value,
                session.DestinationIp.Value,
                session.SourcePort?.Value,
                session.DestinationPort?.Value,
                session.Protocol.Value);
    }
}
