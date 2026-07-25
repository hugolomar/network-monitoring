using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Probe.Application.Configuration;
using NetworkMonitoring.Probe.Application.Models;
using NetworkMonitoring.Probe.Application.Ports;

namespace NetworkMonitoring.Probe.Infrastructure.Traffic;

/// <summary>
/// Provides deterministic traffic observations by reading a configured PCAP file via <c>tshark</c>.
/// </summary>
public sealed class PcapFileTrafficProvider(
    IOptions<ProbeOptions> options,
    TsharkObservationMapper mapper,
    IProbeFlowTelemetry flowTelemetry,
    ILogger<PcapFileTrafficProvider> logger) : ITrafficProvider
{
    /// <summary>
    /// Reads all observations from the configured deterministic PCAP source.
    /// </summary>
    /// <param name="cancellationToken">A token that can stop the PCAP replay consumption loop.</param>
    /// <returns>An asynchronous stream of mapped <see cref="TrafficObservation"/> objects.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when deterministic mode is selected without a valid PCAP path.
    /// </exception>
    public async IAsyncEnumerable<TrafficObservation> ReadObservations(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var probeOptions = options.Value;
        if (string.IsNullOrWhiteSpace(probeOptions.DeterministicTestPcapPath))
        {
            throw new InvalidOperationException(
                "Probe:DeterministicTestPcapPath is required when Probe:InputMode is DeterministicTest.");
        }

        var args = BuildArguments(probeOptions);
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = probeOptions.TSharkPath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        if (!process.Start())
        {
            throw new InvalidOperationException("Unable to start tshark process for deterministic PCAP mode.");
        }

        _ = Task.Run(async () =>
        {
            while (!process.StandardError.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var error = await process.StandardError.ReadLineAsync(cancellationToken);
                if (!string.IsNullOrWhiteSpace(error))
                {
                    if (TsharkCaptureDiagnostics.TryReadCaptureDrops(error, out var dropped))
                    {
                        flowTelemetry.TrackCaptureDropped(dropped);
                    }

                    logger.LogDebug("tshark (pcap): {Error}", error);
                }
            }
        }, cancellationToken);

        DateTimeOffset? previousObservedAtUtc = null;
        var playbackSpeed = probeOptions.DeterministicPlaybackSpeed;

        while (!process.StandardOutput.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await process.StandardOutput.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                continue;
            }

            if (!mapper.TryMap(line, out var observation) || observation is null)
            {
                flowTelemetry.TrackUnparsableInput();
                logger.LogWarning("Skipping malformed tshark PCAP line: {Line}", line);
                continue;
            }

            flowTelemetry.TrackPacketReceived();

            if (playbackSpeed > 0 && previousObservedAtUtc is not null)
            {
                var delta = observation.ObservedAtUtc - previousObservedAtUtc.Value;
                if (delta > TimeSpan.Zero)
                {
                    var delay = TimeSpan.FromTicks((long)(delta.Ticks / playbackSpeed));
                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay, cancellationToken);
                    }
                }
            }

            previousObservedAtUtc = observation.ObservedAtUtc;
            yield return observation;
        }

        if (!process.HasExited)
        {
            process.Kill(true);
        }
    }

    private static string BuildArguments(ProbeOptions options)
    {
        var pcapPath = options.DeterministicTestPcapPath!.Replace("\"", "\\\"");

        // Keep the output fields aligned with TsharkTrafficProvider so the same mapper and
        // downstream use-case behavior are reused in deterministic validation mode.
        var fieldArgs =
            "-T fields " +
            "-e ip.src -e ip.dst -e tcp.srcport -e tcp.dstport -e ip.proto " +
            "-e frame.time_epoch -e frame.len -e eth.src -e eth.dst -e dhcp.option.hostname";

        return $"-r \"{pcapPath}\" {fieldArgs}".Trim();
    }
}
