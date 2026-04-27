using System.Net;
using NetworkMonitoring.IntegrationConsole.Application.Models;
using NetworkMonitoring.IntegrationConsole.Infrastructure.Backend;

namespace NetworkMonitoring.IntegrationConsole.UnitTests.Application.UseCases;

public sealed class DeviceIntakeFailureClassificationTests
{
    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.UnprocessableEntity)]
    public void ClassifyFinal_treats_validation_errors_as_permanent_rejections(HttpStatusCode statusCode)
    {
        var policy = new DeviceIntakeRetryPolicy();

        var outcome = policy.ClassifyFinal(statusCode, 1, "validation failed");

        Assert.Equal(IngestionOutcomeKind.Rejected, outcome.Kind);
        Assert.Equal((int)statusCode, outcome.StatusCode);
    }
}
