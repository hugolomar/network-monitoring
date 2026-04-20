using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Probe.Application.Configuration;
using NetworkMonitoring.Probe.Application.Models;
using NetworkMonitoring.Probe.Application.Ports;

namespace NetworkMonitoring.Probe.Infrastructure.Traffic;

public sealed class TsharkTrafficProvider(
    IOptions<ProbeOptions> options,
    TsharkObservationMapper mapper,
    ILogger<TsharkTrafficProvider> logger) : ITrafficProvider
{
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

        _ = Task.Run(async () =>
        {
            while (!process.StandardError.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var error = await process.StandardError.ReadLineAsync(cancellationToken);
                if (!string.IsNullOrWhiteSpace(error))
                {
                    logger.LogDebug("tshark: {Error}", error);
                }
            }
        }, cancellationToken);

        while (!process.StandardOutput.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await process.StandardOutput.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                continue;
            }

            if (!mapper.TryMap(line, out var observation) || observation is null)
            {
                logger.LogWarning("Skipping malformed tshark line: {Line}", line);
                continue;
            }

            yield return observation;
        }

        if (!process.HasExited)
        {
            process.Kill(true);
        }
    }

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
