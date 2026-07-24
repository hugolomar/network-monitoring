using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetworkMonitoring.Backend.Infrastructure.Graph;
using NetworkMonitoring.Backend.Infrastructure.Persistence;

namespace NetworkMonitoring.Backend.Host.Health;

/// <summary>
/// Verifies backend dependencies required for request readiness.
/// </summary>
internal sealed class BackendReadinessHealthCheck(
    DeviceInventoryDbContext dbContext,
    Neo4jDriverAccessor neo4jDriverAccessor) : IHealthCheck
{
    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!await dbContext.Database.CanConnectAsync(cancellationToken))
        {
            return HealthCheckResult.Unhealthy("PostgreSQL connectivity check failed.");
        }

        await neo4jDriverAccessor.GetOrCreate().VerifyConnectivityAsync();
        return HealthCheckResult.Healthy();
    }
}
