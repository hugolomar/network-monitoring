using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetworkMonitoring.IntegrationConsole.Host.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddIntegrationConsole(builder.Configuration);

await builder.Build().RunAsync();

/// <summary>
/// Entry point for the Integration Console application.
/// </summary>
public partial class Program { }
