using NetworkMonitoring.Probe.Host.DependencyInjection;
using NetworkMonitoring.Probe.Host.Services;

// Entry point for the Network Monitoring Probe application.
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddProbeServices(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddHostedService<ProbeWorker>();

var host = builder.Build();
host.Run();
