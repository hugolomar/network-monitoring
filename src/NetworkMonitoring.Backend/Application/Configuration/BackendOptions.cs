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

    /// <summary>
    /// Gets communication graph feature options.
    /// </summary>
    public GraphOptions Graph { get; init; } = new();
}

/// <summary>
/// Configuration values for communication graph behavior.
/// </summary>
public sealed class GraphOptions
{
    /// <summary>
    /// Gets graph provider kind ("Neo4j" or "InMemory").
    /// </summary>
    public string Provider { get; init; } = "Neo4j";

    /// <summary>
    /// Gets Neo4j bolt URI.
    /// </summary>
    public string Neo4jUri { get; init; } = "bolt://localhost:7687";

    /// <summary>
    /// Gets Neo4j user name.
    /// </summary>
    public string Neo4jUsername { get; init; } = "neo4j";

    /// <summary>
    /// Gets Neo4j password.
    /// </summary>
    public string Neo4jPassword { get; init; } = "networkmonitoring123";

    /// <summary>
    /// Gets the default traversal depth used when omitted by callers.
    /// </summary>
    public int DefaultDepth { get; init; } = 1;

    /// <summary>
    /// Gets the maximum traversal depth accepted by the API.
    /// </summary>
    public int MaxDepth { get; init; } = 3;

    /// <summary>
    /// Gets the default node limit used when omitted by callers.
    /// </summary>
    public int DefaultLimit { get; init; } = 200;

    /// <summary>
    /// Gets the maximum node limit accepted by the API.
    /// </summary>
    public int MaxLimit { get; init; } = 500;

    /// <summary>
    /// Gets the retention window in days.
    /// </summary>
    public int RetentionWindowDays { get; init; } = 90;

    /// <summary>
    /// Gets the retention execution cadence in hours.
    /// </summary>
    public int RetentionCadenceHours { get; init; } = 24;

    /// <summary>
    /// Gets the maximum number of projection retries.
    /// </summary>
    public int ProjectionMaxRetries { get; init; } = 3;

    /// <summary>
    /// Gets the base delay, in milliseconds, for projection retries.
    /// </summary>
    public int ProjectionRetryBaseDelayMs { get; init; } = 200;

    /// <summary>
    /// Gets per-attempt timeout, in milliseconds, for projection writes.
    /// </summary>
    public int ProjectionAttemptTimeoutMs { get; init; } = 2000;

    /// <summary>
    /// Gets a value indicating whether structured graph logs are enabled.
    /// </summary>
    public bool EnableStructuredLogs { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether graph metrics are enabled.
    /// </summary>
    public bool EnableMetrics { get; init; } = true;
}
