using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Backend.Infrastructure.Graph;
using NetworkMonitoring.Backend.Host.Telemetry;

namespace NetworkMonitoring.Backend.Host.Endpoints;

/// <summary>
/// Graph API endpoints.
/// </summary>
public static class GraphEndpoints
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin",
        "analyst",
        "auditor",
        "integration"
    };

    /// <summary>
    /// Maps graph retrieval endpoints.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder where graph endpoints are registered.</param>
    /// <returns>The same route builder instance for chaining.</returns>
    public static IEndpointRouteBuilder MapGraphEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/graph/devices", GetDevicesGraph)
            .Produces<GraphDevicesResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status503ServiceUnavailable);
        endpoints.MapGet("/api/graph/devices/all", GetDevicesGraphSnapshot)
            .Produces<GraphDevicesResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status503ServiceUnavailable);

        return endpoints;
    }

    private static async Task<IResult> GetDevicesGraph(
        HttpContext httpContext,
        [AsParameters] GraphDevicesRequestDto request,
        GetDeviceGraphUseCase useCase,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        if (!httpContext.Request.Headers.ContainsKey("Authorization"))
        {
            return Results.Unauthorized();
        }

        var callerRole = httpContext.Request.Headers["X-Role"].FirstOrDefault() ?? string.Empty;
        if (!AllowedRoles.Contains(callerRole))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        try
        {
            var result = await useCase.Execute(request.RootDeviceId, request.Depth, request.Limit, cancellationToken);
            var response = new GraphDevicesResponseDto(
                result.Nodes.Select(node => new GraphNodeDto(node.Id, node.Kind)).ToArray(),
                result.Edges.Select(edge => new GraphEdgeDto(
                    edge.SourceId,
                    edge.DestinationId,
                    edge.Protocol,
                    edge.Weight,
                    edge.FirstSeenUtc,
                    edge.LastSeenUtc)).ToArray(),
                result.Truncated);

            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(GraphErrorResponses.InvalidRequest(ex.Message, httpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            var logger = loggerFactory.CreateLogger("GraphEndpoints");
            var correlationId = httpContext.Items["X-Correlation-ID"]?.ToString()
                ?? httpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                ?? string.Empty;
            logger.LogError(
                ex,
                "Graph retrieval failed for rootDeviceId {RootDeviceId}. correlationId={CorrelationId} traceId={TraceId} errorContext={ErrorContext}",
                request.RootDeviceId,
                correlationId,
                System.Diagnostics.Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier,
                TelemetryRedaction.Redact(ex.Message));
            _ = Neo4jGraphErrorMapper.Map(ex);
            return Results.Json(
                GraphErrorResponses.GraphUnavailable("Communication graph is temporarily unavailable.", httpContext.TraceIdentifier),
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    private static async Task<IResult> GetDevicesGraphSnapshot(
        HttpContext httpContext,
        [AsParameters] GraphSnapshotRequestDto request,
        GetDeviceGraphUseCase useCase,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        if (!httpContext.Request.Headers.ContainsKey("Authorization"))
        {
            return Results.Unauthorized();
        }

        var callerRole = httpContext.Request.Headers["X-Role"].FirstOrDefault() ?? string.Empty;
        if (!AllowedRoles.Contains(callerRole))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        try
        {
            var result = await useCase.ExecuteSnapshot(request.Limit, cancellationToken);
            var response = new GraphDevicesResponseDto(
                result.Nodes.Select(node => new GraphNodeDto(node.Id, node.Kind)).ToArray(),
                result.Edges.Select(edge => new GraphEdgeDto(
                    edge.SourceId,
                    edge.DestinationId,
                    edge.Protocol,
                    edge.Weight,
                    edge.FirstSeenUtc,
                    edge.LastSeenUtc)).ToArray(),
                result.Truncated);

            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(GraphErrorResponses.InvalidRequest(ex.Message, httpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            var logger = loggerFactory.CreateLogger("GraphEndpoints");
            var correlationId = httpContext.Items["X-Correlation-ID"]?.ToString()
                ?? httpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                ?? string.Empty;
            logger.LogError(
                ex,
                "Graph snapshot retrieval failed. correlationId={CorrelationId} traceId={TraceId} errorContext={ErrorContext}",
                correlationId,
                System.Diagnostics.Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier,
                TelemetryRedaction.Redact(ex.Message));
            _ = Neo4jGraphErrorMapper.Map(ex);
            return Results.Json(
                GraphErrorResponses.GraphUnavailable("Communication graph is temporarily unavailable.", httpContext.TraceIdentifier),
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
