using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Observability.CircuitBreaker;

/// <summary>
/// Extension methods for registering circuit breaker infrastructure.
/// </summary>
public static class CircuitBreakerExtensions
{
    /// <summary>
    /// Registers <see cref="CircuitBreakerRegistry"/> and <see cref="CircuitBreakerMetrics"/>
    /// as singletons in the DI container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="failureThreshold">
    /// Optional failure threshold passed to <see cref="CircuitBreakerRegistry"/>.
    /// Defaults to <see cref="CircuitBreakerRegistry.DefaultFailureThreshold"/>.
    /// </param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddPlatformCircuitBreakerRegistry(
        this IServiceCollection services,
        int failureThreshold = CircuitBreakerRegistry.DefaultFailureThreshold
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton(new CircuitBreakerRegistry(failureThreshold));
        services.AddSingleton<CircuitBreakerMetrics>(sp => new CircuitBreakerMetrics(
            sp.GetRequiredService<CircuitBreakerRegistry>()
        ));
        return services;
    }

    /// <summary>
    /// Maps circuit breaker dashboard endpoints, resolving the <see cref="CircuitBreakerRegistry"/>
    /// from the application's service provider.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    /// <returns>The same <paramref name="endpoints"/> for chaining.</returns>
    public static IEndpointRouteBuilder MapCircuitBreakerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        var registry = endpoints.ServiceProvider.GetRequiredService<CircuitBreakerRegistry>();
        return CircuitBreakerEndpoints.MapCircuitBreakerEndpoints(endpoints, registry);
    }
}
