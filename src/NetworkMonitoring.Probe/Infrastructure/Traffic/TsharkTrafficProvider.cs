using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Probe.Application.Configuration;
using NetworkMonitoring.Probe.Application.Models;
using NetworkMonitoring.Probe.Application.Ports;

namespace NetworkMonitoring.Probe.Infrastructure.Traffic;

/// <summary>
/// Provides a stream of network traffic observations by orchestrating an external 'tshark' (Wireshark) process.
/// This implementation captures real-time packets and translates them into domain-neutral observations.
/// </summary>
public sealed class TsharkTrafficProvider(
    IOptions<ProbeOptions> options,
    TsharkObservationMapper mapper,
    IProbeFlowTelemetry flowTelemetry,
    ILogger<TsharkTrafficProvider> logger) : ITrafficProvider
{
    /// <summary>
    /// Starts the tshark capture process and streams mapped traffic observations.
    /// </summary>
    /// <param name="cancellationToken">A token to signal the termination of the capture process.</param>
    /// <returns>An asynchronous stream of <see cref="TrafficObservation"/> objects.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the tshark executable cannot be started.</exception>
    public async IAsyncEnumerable<TrafficObservation> ReadObservations(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var probeOptions = options.Value;
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
            throw new InvalidOperationException("Unable to start tshark process.");
        }

        // We consume the error stream in a separate background task to prevent the process 
        // from hanging due to a full buffer (deadlock prevention), while allowing the 
        // main loop to focus on processing standard output.
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

                    logger.LogDebug("tshark: {Error}", error);
                }
            }
        }, cancellationToken);

        // Main processing loop: consumes the tab-separated output from tshark.
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
                logger.LogWarning("Skipping malformed tshark line: {Line}", line);
                continue;
            }

            flowTelemetry.TrackPacketReceived();
            yield return observation;
        }

        // Ensure cleanup by killing the process if the stream is disposed before natural termination.
        if (!process.HasExited)
        {
            process.Kill(true);
        }
    }

    /// <summary>
    /// Constructs the command-line arguments for tshark.
    /// </summary>
    /// <remarks>
    /// Configuration rationale:
    /// - '-l' enables line-buffered output for real-time streaming.
    /// - '-T fields' combined with '-e' extractions provides a high-performance tab-separated 
    ///   output format, minimizing parsing complexity.
    /// </remarks>
    private static string BuildArguments(ProbeOptions options)
    {
        // Emit tab-separated fields that map directly to TrafficObservation.
        var fieldArgs =
            "-l -T fields " +
            "-e ip.src -e ip.dst -e tcp.srcport -e tcp.dstport -e ip.proto " +
            "-e frame.time_epoch -e frame.len -e eth.src -e eth.dst -e dhcp.option.hostname";

        var interfaceArg = string.IsNullOrWhiteSpace(options.InterfaceName)
            ? string.Empty
            : $"-i {options.InterfaceName}";

        var captureFilterArg = string.IsNullOrWhiteSpace(options.CaptureFilter)
            ? string.Empty
            : $"-f \"{options.CaptureFilter}\"";

        return $"{interfaceArg} {captureFilterArg} {fieldArgs}".Trim();
    }
}
