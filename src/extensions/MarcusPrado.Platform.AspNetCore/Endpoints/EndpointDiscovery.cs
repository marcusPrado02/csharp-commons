using System.Reflection;

namespace MarcusPrado.Platform.AspNetCore.Endpoints;

/// <summary>
/// Discovers all <see cref="IEndpoint"/> implementations in given assemblies
/// via reflection and calls <see cref="IEndpoint.MapEndpoints"/> on each.
/// </summary>
public static class EndpointDiscovery
{
    /// <summary>
    /// Scans <paramref name="assemblies"/> (or the calling assembly when empty)
    /// for concrete <see cref="IEndpoint"/> types and maps their endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapPlatformEndpoints(
        this IEndpointRouteBuilder app,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(app);

        var targets = assemblies.Length > 0
            ? assemblies
            : [Assembly.GetCallingAssembly()];

        foreach (var assembly in targets)
        {
            var types = assembly.GetExportedTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false }
                             && typeof(IEndpoint).IsAssignableFrom(t));

            foreach (var type in types)
            {
                if (Activator.CreateInstance(type) is IEndpoint endpoint)
                {
                    endpoint.MapEndpoints(app);
                }
            }
        }

        return app;
    }
}
