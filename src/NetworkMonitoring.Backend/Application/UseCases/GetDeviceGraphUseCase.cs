using Microsoft.Extensions.Options;
using NetworkMonitoring.Backend.Application.Configuration;
using NetworkMonitoring.Backend.Application.Models;
using NetworkMonitoring.Backend.Application.Ports;

namespace NetworkMonitoring.Backend.Application.UseCases;

/// <summary>
/// Retrieves a bounded graph neighborhood for one root identity.
/// </summary>
public sealed class GetDeviceGraphUseCase(
    IGraphQueryRepository graphQueryRepository,
    IOptions<BackendOptions> options)
{
    /// <summary>
    /// Executes bounded graph retrieval with defaults and caps from configuration.
    /// </summary>
    /// <param name="rootDeviceId">The root graph identity to traverse from.</param>
    /// <param name="depth">Optional caller-requested traversal depth.</param>
    /// <param name="limit">Optional caller-requested maximum node count.</param>
    /// <param name="cancellationToken">The cancellation token used to abort execution.</param>
    /// <returns>A bounded graph neighborhood result.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="rootDeviceId"/> is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when effective depth or limit is less than one.
    /// </exception>
    public Task<GraphQueryResult> Execute(
        string rootDeviceId,
        int? depth,
        int? limit,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rootDeviceId))
        {
            throw new ArgumentException("rootDeviceId is required.", nameof(rootDeviceId));
        }

        var graph = options.Value.Graph;
        var effectiveDepth = depth ?? graph.DefaultDepth;
        var effectiveLimit = limit ?? graph.DefaultLimit;

        if (effectiveDepth < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(depth), "depth must be >= 1.");
        }

        if (effectiveLimit < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "limit must be >= 1.");
        }

        if (effectiveDepth > graph.MaxDepth)
        {
            effectiveDepth = graph.MaxDepth;
        }

        if (effectiveLimit > graph.MaxLimit)
        {
            effectiveLimit = graph.MaxLimit;
        }

        return graphQueryRepository.GetDeviceGraph(rootDeviceId, effectiveDepth, effectiveLimit, cancellationToken);
    }

    /// <summary>
    /// Executes bounded full-graph retrieval with configured default/cap semantics.
    /// </summary>
    /// <param name="limit">Optional caller-requested maximum node count.</param>
    /// <param name="cancellationToken">The cancellation token used to abort execution.</param>
    /// <returns>A bounded graph snapshot result.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when effective limit is less than one.</exception>
    public Task<GraphQueryResult> ExecuteSnapshot(
        int? limit,
        CancellationToken cancellationToken)
    {
        var graph = options.Value.Graph;
        var effectiveLimit = limit ?? graph.DefaultLimit;

        if (effectiveLimit < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "limit must be >= 1.");
        }

        if (effectiveLimit > graph.MaxLimit)
        {
            effectiveLimit = graph.MaxLimit;
        }

        return graphQueryRepository.GetGraphSnapshot(effectiveLimit, cancellationToken);
    }
}
