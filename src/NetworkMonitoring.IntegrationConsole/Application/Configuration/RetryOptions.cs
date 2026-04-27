namespace NetworkMonitoring.IntegrationConsole.Application.Configuration;

public sealed record RetryOptions(int MaxAttempts, TimeSpan BaseDelay)
{
    public static RetryOptions From(IntegrationConsoleOptions options) =>
        new(
            Math.Max(1, options.RetryMaxAttempts),
            TimeSpan.FromMilliseconds(Math.Max(0, options.RetryBaseDelayMilliseconds)));
}
