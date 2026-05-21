namespace NetworkMonitoring.Backend.Application.Configuration;

/// <summary>
/// Represents the configuration options for the backend application.
/// </summary>
public sealed class BackendOptions
{
    /// <summary>
    /// The name of the configuration section.
    /// </summary>
    public const string SectionName = "Backend";

    /// <summary>
    /// Gets the connection string for the database.
    /// </summary>
    public string ConnectionString { get; init; } =
        "Host=localhost;Port=5432;Database=network_monitoring;Username=network_monitoring;Password=network_monitoring";

    /// <summary>
    /// Gets a value indicating whether to apply database migrations on application startup.
    /// </summary>
    public bool ApplyMigrationsOnStartup { get; init; } = true;
}
