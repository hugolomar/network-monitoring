using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Probe.Application.Configuration;
using NetworkMonitoring.Probe.Application.UseCases;
using NetworkMonitoring.Probe.Infrastructure.Publishing;
using NetworkMonitoring.Probe.Infrastructure.Traffic;
using System.Diagnostics;
using System.Text.Json;

namespace NetworkMonitoring.Probe.IntegrationTests;

/// <summary>
/// Integration coverage for deterministic test input mode based on PCAP files.
/// </summary>
public sealed class DeterministicPcapTrafficProviderIntegrationTests
{
    /// <summary>
    /// Verifies deterministic input mode emits session detections through the normal use-case pipeline.
    /// </summary>
    [SkippableFact]
    public async Task ExecuteAsync_WithDeterministicPcapInput_EmitsSessionDetectedRecords()
    {
        var pcapPath = ResolveDeterministicFixturePcapPath();
        Skip.IfNot(File.Exists(pcapPath), $"PCAP fixture not found: {pcapPath}");
        Skip.IfNot(IsCommandAvailable("tshark"), "tshark is required to run deterministic PCAP integration tests.");

        var output = await ExecuteUseCaseAndCaptureConsole(pcapPath);
        var sessionEvents = ExtractSessionFingerprints(output);

        Assert.NotEmpty(sessionEvents);
    }

    /// <summary>
    /// Verifies two deterministic mode runs produce equivalent sampled session required fields.
    /// </summary>
    [SkippableFact]
    public async Task ExecuteAsync_WithSameDeterministicPcapInput_ProducesEquivalentSampleAcrossRuns()
    {
        var pcapPath = ResolveDeterministicFixturePcapPath();
        Skip.IfNot(File.Exists(pcapPath), $"PCAP fixture not found: {pcapPath}");
        Skip.IfNot(IsCommandAvailable("tshark"), "tshark is required to run deterministic PCAP integration tests.");

        var firstRun = ExtractSessionFingerprints(await ExecuteUseCaseAndCaptureConsole(pcapPath))
            .Take(5)
            .ToArray();
        var secondRun = ExtractSessionFingerprints(await ExecuteUseCaseAndCaptureConsole(pcapPath))
            .Take(5)
            .ToArray();

        Assert.NotEmpty(firstRun);
        Assert.Equal(firstRun, secondRun);
    }

    private static async Task<string> ExecuteUseCaseAndCaptureConsole(string pcapPath)
    {
        var probeOptions = Options.Create(
            new ProbeOptions
            {
                InputMode = CaptureInputMode.DeterministicTest,
                DeterministicTestPcapPath = pcapPath,
                TSharkPath = "tshark",
                SessionDeduplicationWindowMinutes = 0,
                DeviceDeduplicationWindowMinutes = 0,
            });

        var trafficProvider = new PcapFileTrafficProvider(
            probeOptions,
            new TsharkObservationMapper(),
            NullLogger<PcapFileTrafficProvider>.Instance);

        var useCase = new ProcessObservationsUseCase(
            trafficProvider,
            new ConsolePublisher(new ConsoleRecordSerializer()),
            probeOptions,
            NullLogger<ProcessObservationsUseCase>.Instance);

        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            await useCase.ExecuteAsync(CancellationToken.None);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        return writer.ToString();
    }

    private static IReadOnlyList<string> ExtractSessionFingerprints(string output)
    {
        var fingerprints = new List<string>();
        var lines = output
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var line in lines)
        {
            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;
            if (!root.TryGetProperty("eventType", out var eventType)
                || !string.Equals(eventType.GetString(), "SessionDetected", StringComparison.Ordinal))
            {
                continue;
            }

            var sourceIp = root.GetProperty("sourceIp").GetString() ?? string.Empty;
            var destinationIp = root.GetProperty("destinationIp").GetString() ?? string.Empty;
            var sourcePort = root.TryGetProperty("sourcePort", out var sourcePortElement)
                ? sourcePortElement.ToString()
                : string.Empty;
            var destinationPort = root.TryGetProperty("destinationPort", out var destinationPortElement)
                ? destinationPortElement.ToString()
                : string.Empty;
            var protocol = root.GetProperty("protocol").GetString() ?? string.Empty;
            var bytesObserved = root.GetProperty("bytesObserved").ToString();

            fingerprints.Add(
                $"{sourceIp}|{destinationIp}|{sourcePort}|{destinationPort}|{protocol}|{bytesObserved}");
        }

        return fingerprints;
    }

    private static string ResolveDeterministicFixturePcapPath()
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
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"-lc \"command -v {command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        if (!process.Start())
        {
            return false;
        }

        process.WaitForExit();
        return process.ExitCode == 0;
    }
}
