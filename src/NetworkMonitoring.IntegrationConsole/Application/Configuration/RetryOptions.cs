namespace NetworkMonitoring.IntegrationConsole.Application.Configuration;

/// <summary>
/// Options for configuring retry behavior.
/// </summary>
/// <param name="MaxAttempts">Maximum number of attempts.</param>
/// <param name="BaseDelay">Base delay between attempts.</param>
public sealed record RetryOptions(int MaxAttempts, TimeSpan BaseDelay)
{
    /// <summary>
    /// Creates retry options from application configuration.
    /// </summary>
    /// <param name="options">The application options.</param>
    /// <returns>A new <see cref="RetryOptions"/> instance.</returns>
    public static RetryOptions From(IntegrationConsoleOptions options) =>
        new(
            Math.Max(1, options.RetryMaxAttempts),
            TimeSpan.FromMilliseconds(Math.Max(0, options.RetryBaseDelayMilliseconds)));
}
