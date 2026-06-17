using System.Reflection;
using NetworkMonitoring.Backend.Application.UseCases;

namespace NetworkMonitoring.Backend.UnitTests.Architecture;

/// <summary>
/// Test suite for BackendArchitecture.
/// </summary>
public sealed class BackendArchitectureTests
{
    /// <summary>
    /// Verifies that application layer does not depend on infrastructure or host.
    /// </summary>
    [Fact]
    public void Application_layer_does_not_depend_on_infrastructure_or_host()
    {
        var applicationAssembly = typeof(AcceptDeviceIntakeUseCase).Assembly;
        var applicationTypes = applicationAssembly
            .GetTypes()
            .Where(type => type.Namespace?.Contains(".Application.", StringComparison.Ordinal) is true);

        var forbiddenReferences = applicationTypes
            .SelectMany(GetReferencedTypes)
            .Where(type => type.Namespace?.Contains(".Infrastructure.", StringComparison.Ordinal) is true
                || type.Namespace?.Contains(".Host.", StringComparison.Ordinal) is true)
            .Select(type => type.FullName)
            .Distinct()
            .Order()
            .ToArray();

        Assert.Empty(forbiddenReferences);
    }

    private static IEnumerable<Type> GetReferencedTypes(Type type)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        return type.GetFields(flags).Select(field => field.FieldType)
            .Concat(type.GetProperties(flags).Select(property => property.PropertyType))
            .Concat(type.GetMethods(flags).Select(method => method.ReturnType))
            .Concat(type.GetConstructors(flags).SelectMany(ctor => ctor.GetParameters().Select(parameter => parameter.ParameterType)));
    }
}
