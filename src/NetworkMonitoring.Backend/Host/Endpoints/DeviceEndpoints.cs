using NetworkMonitoring.Backend.Application.Models;
using NetworkMonitoring.Backend.Application.UseCases;

namespace NetworkMonitoring.Backend.Host.Endpoints;

/// <summary>
/// Defines the HTTP endpoints for device management.
/// </summary>
public static class DeviceEndpoints
{
    /// <summary>
    /// Maps the device management endpoints to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to map the endpoints to.</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> so that additional calls can be chained.</returns>
    public static IEndpointRouteBuilder MapDeviceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/devices", AcceptDevice)
            .Accepts<DeviceIntakeRequestDto>("application/json")
            .Produces<DeviceIntakeResponseDto>(StatusCodes.Status201Created)
            .Produces<DeviceIntakeResponseDto>(StatusCodes.Status200OK)
            .Produces<DeviceIntakeResponseDto>(StatusCodes.Status400BadRequest)
            .Produces<DeviceIntakeResponseDto>(StatusCodes.Status415UnsupportedMediaType)
            .Produces<DeviceIntakeResponseDto>(StatusCodes.Status503ServiceUnavailable);

        endpoints.MapGet("/devices", ListDevices)
            .Produces<DeviceInventoryResponseDto>(StatusCodes.Status200OK);

        return endpoints;
    }

    private static async Task<IResult> AcceptDevice(
            HttpRequest httpRequest,
            DeviceIntakeRequestDto? request,
            AcceptDeviceIntakeUseCase useCase,
            CancellationToken cancellationToken)
    {
        // Validate that the request uses the correct media type
        if (!httpRequest.HasJsonContentType())
        {
            return Results.Json(
                new DeviceIntakeResponseDto("rejected", "Content-Type must be application/json.", null),
                statusCode: StatusCodes.Status415UnsupportedMediaType);
        }

        if (request is null)
        {
            return TypedResults.BadRequest(new DeviceIntakeResponseDto("rejected", "Request body is required.", null));
        }

        // Map the request to a command, extracting the idempotency key if provided
        var command = DeviceIntakeRequestMapper.ToCommand(request, httpRequest.Headers["Idempotency-Key"].FirstOrDefault());
        var outcome = await useCase.Execute(command, cancellationToken);
        var response = new DeviceIntakeResponseDto(outcome.Kind.ToString(), outcome.Reason, outcome.Device);

        // Map the use case outcome to the appropriate HTTP response
        return outcome.Kind switch
        {
            DeviceIntakeOutcomeKind.Created => Results.Created($"/devices/{outcome.Device!.Id}", response),
            DeviceIntakeOutcomeKind.Updated or DeviceIntakeOutcomeKind.Idempotent => Results.Ok(response),
            DeviceIntakeOutcomeKind.PersistenceFailure => Results.Json(response, statusCode: StatusCodes.Status503ServiceUnavailable),
            _ => Results.BadRequest(response)
        };
    }

    private static async Task<IResult> ListDevices(
        ListDevicesUseCase useCase,
        CancellationToken cancellationToken)
    {
        var devices = await useCase.Execute(cancellationToken);
        return Results.Ok(new DeviceInventoryResponseDto(devices));
    }
}
