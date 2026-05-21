using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetworkMonitoring.IntegrationConsole.Host.DependencyInjection;

/// <summary>
/// Entry point for the Integration Console application.
/// </summary>
public partial class Program { }

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddIntegrationConsole(builder.Configuration);

await builder.Build().RunAsync();
