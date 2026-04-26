using NetworkMonitoring.Probe.Host.DependencyInjection;
using NetworkMonitoring.Probe.Host.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddProbeServices(builder.Configuration);
builder.Services.AddHostedService<ProbeWorker>();

var host = builder.Build();
host.Run();
