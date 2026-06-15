using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Backend.Infrastructure.Graph;

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
            logger.LogError(ex, "Graph retrieval failed for rootDeviceId {RootDeviceId}", request.RootDeviceId);
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
            logger.LogError(ex, "Graph snapshot retrieval failed.");
            _ = Neo4jGraphErrorMapper.Map(ex);
            return Results.Json(
                GraphErrorResponses.GraphUnavailable("Communication graph is temporarily unavailable.", httpContext.TraceIdentifier),
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
