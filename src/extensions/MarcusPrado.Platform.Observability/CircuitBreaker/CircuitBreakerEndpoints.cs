using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MarcusPrado.Platform.Observability.CircuitBreaker;

/// <summary>
/// Minimal API endpoint registration for the circuit breaker dashboard.
/// </summary>
public static class CircuitBreakerEndpoints
{
    /// <summary>
    /// Maps the circuit breaker dashboard endpoints onto the provided <paramref name="endpoints"/> builder.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><c>GET  /circuit-breakers</c>       — returns a JSON array of all <see cref="CircuitBreakerEntry"/> values.</item>
    /// <item><c>POST /circuit-breakers/{name}/reset</c> — resets the named breaker to Closed with zero failures.</item>
    /// </list>
    /// </remarks>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    /// <param name="registry">The <see cref="CircuitBreakerRegistry"/> that holds the state.</param>
    /// <returns>The same <paramref name="endpoints"/> builder for chaining.</returns>
    public static IEndpointRouteBuilder MapCircuitBreakerEndpoints(
        this IEndpointRouteBuilder endpoints,
        CircuitBreakerRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(registry);

        endpoints.MapGet("/circuit-breakers", () =>
            Results.Ok(registry.GetAll()));

        endpoints.MapPost("/circuit-breakers/{name}/reset", (string name) =>
        {
            registry.Reset(name);
            return Results.Ok();
        });

        return endpoints;
    }
}
