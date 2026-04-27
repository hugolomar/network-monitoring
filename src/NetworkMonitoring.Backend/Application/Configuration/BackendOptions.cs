namespace NetworkMonitoring.Backend.Application.Configuration;

public sealed class BackendOptions
{
    public const string SectionName = "Backend";

    public string ConnectionString { get; init; } =
        "Host=localhost;Port=5432;Database=network_monitoring;Username=network_monitoring;Password=network_monitoring";

    public bool ApplyMigrationsOnStartup { get; init; } = true;
}
