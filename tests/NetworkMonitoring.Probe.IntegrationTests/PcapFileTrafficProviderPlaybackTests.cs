using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Probe.Application.Configuration;
using NetworkMonitoring.Probe.Infrastructure.Traffic;
using System.Diagnostics;

namespace NetworkMonitoring.Probe.IntegrationTests;

/// <summary>
/// Integration coverage for deterministic PCAP playback pacing.
/// </summary>
public sealed class PcapFileTrafficProviderPlaybackTests
{
    /// <summary>
    /// Verifies zero playback speed reads observations without intentional pacing delay.
    /// </summary>
    [SkippableFact]
    public async Task ReadObservations_WithZeroPlaybackSpeed_CompletesQuickly()
    {
        var sourcePcap = ResolveHttpFixturePcapPath();
        Skip.IfNot(File.Exists(sourcePcap), $"PCAP fixture not found: {sourcePcap}");
        Skip.IfNot(IsCommandAvailable("tshark"), "tshark is required for playback integration tests.");

        var provider = CreateProvider(sourcePcap, playbackSpeed: 0);
        var stopwatch = Stopwatch.StartNew();
        var count = 0;

        await foreach (var _ in provider.ReadObservations(CancellationToken.None))
        {
            count++;
        }

        stopwatch.Stop();

        Assert.True(count > 0);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(10));
    }

    /// <summary>
    /// Verifies real-time playback waits according to packet timestamp spacing.
    /// </summary>
    [SkippableFact]
    public async Task ReadObservations_WithRealTimePlayback_WaitsBetweenTimestampedPackets()
    {
        Skip.IfNot(IsCommandAvailable("tshark"), "tshark is required for playback integration tests.");
        Skip.IfNot(IsCommandAvailable("editcap"), "editcap is required for playback integration tests.");
        Skip.IfNot(IsCommandAvailable("mergecap"), "mergecap is required for playback integration tests.");

        var sourcePcap = ResolveHttpFixturePcapPath();
        Skip.IfNot(File.Exists(sourcePcap), $"PCAP fixture not found: {sourcePcap}");

        var pacedPcap = CreateTwoPacketPcapWithSpacing(sourcePcap, spacingSeconds: 1.5);
        try
        {
            var provider = CreateProvider(pacedPcap, playbackSpeed: 1.0);
            var stopwatch = Stopwatch.StartNew();
            var count = 0;

            await foreach (var _ in provider.ReadObservations(CancellationToken.None))
            {
                count++;
            }

            stopwatch.Stop();

            Assert.Equal(2, count);
            Assert.True(stopwatch.Elapsed >= TimeSpan.FromMilliseconds(1200));
        }
        finally
        {
            File.Delete(pacedPcap);
        }
    }

    private static PcapFileTrafficProvider CreateProvider(string pcapPath, double playbackSpeed) =>
        new(
            Options.Create(
                new ProbeOptions
                {
                    InputMode = CaptureInputMode.DeterministicTest,
                    DeterministicTestPcapPath = pcapPath,
                    DeterministicPlaybackSpeed = playbackSpeed,
                    TSharkPath = "tshark",
                }),
            new TsharkObservationMapper(),
            NullLogger<PcapFileTrafficProvider>.Instance);

    private static string CreateTwoPacketPcapWithSpacing(string sourcePcap, double spacingSeconds)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"probe-playback-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        var firstPacket = Path.Combine(tempDir, "first.pcap");
        var secondPacket = Path.Combine(tempDir, "second.pcap");
        var shiftedSecond = Path.Combine(tempDir, "second-shifted.pcap");
        var merged = Path.Combine(tempDir, "paced.pcap");

        RunOrThrow("tshark", $"-r \"{sourcePcap}\" -c 1 -w \"{firstPacket}\"");
        RunOrThrow("tshark", $"-r \"{sourcePcap}\" -Y \"frame.number==2\" -w \"{secondPacket}\"");
        RunOrThrow("editcap", $"-t {spacingSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture)} \"{secondPacket}\" \"{shiftedSecond}\"");
        RunOrThrow("mergecap", $"-w \"{merged}\" \"{firstPacket}\" \"{shiftedSecond}\"");

        return merged;
    }

    private static void RunOrThrow(string fileName, string arguments)
    {
        using var process = Process.Start(
            new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });

        if (process is null)
        {
            throw new InvalidOperationException($"Unable to start process: {fileName}");
        }

        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            var detail = process.StandardError.ReadToEnd();
            throw new InvalidOperationException($"{fileName} failed: {detail}");
        }
    }

    private static string ResolveHttpFixturePcapPath()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null
               && !File.Exists(Path.Combine(current.FullName, "src", "NetworkMonitoring.sln")))
        {
            current = current.Parent;
        }

        if (current is null)
        {
            return string.Empty;
        }

        return Path.Combine(current.FullName, "tools", "traffic", "pcaps", "external", "http.cap");
    }

    private static bool IsCommandAvailable(string command)
    {
        using var process = Process.Start(
            new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"-lc \"command -v {command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });

        if (process is null)
        {
            return false;
        }

        process.WaitForExit();
        return process.ExitCode == 0;
    }
}
