using NetworkMonitoring.Probe.Host.DependencyInjection;
using NetworkMonitoring.Probe.Host.Services;

/// <summary>
/// Entry point for the Network Monitoring Probe application.
/// Initializes the host, registers services, and starts the background capture worker.
/// </summary>

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddProbeServices(builder.Configuration);
builder.Services.AddHostedService<ProbeWorker>();

var host = builder.Build();
host.Run();
