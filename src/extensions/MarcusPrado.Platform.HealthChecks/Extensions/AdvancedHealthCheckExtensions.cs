using MarcusPrado.Platform.HealthChecks.Checks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MarcusPrado.Platform.HealthChecks.Extensions;

/// <summary>
/// Extension methods for registering advanced health check features (T-50).
/// </summary>
public static class AdvancedHealthCheckExtensions
{
    /// <summary>
    /// Registers a <see cref="DegradedHealthCheck"/> under <paramref name="name"/>
    /// that always returns <see cref="HealthStatus.Degraded"/> with the given <paramref name="reason"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to extend.</param>
    /// <param name="name">The unique name for this health check registration.</param>
    /// <param name="reason">The degraded reason message to include in every result.</param>
    /// <returns>The same <see cref="IHealthChecksBuilder"/> for chaining.</returns>
    public static IHealthChecksBuilder AddDegradedHealthCheck(
        this IHealthChecksBuilder builder,
        string name,
        string reason)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddCheck(name, new DegradedHealthCheck(reason));
    }

    /// <summary>
    /// Registers the <see cref="HealthCheckHistory"/> singleton with the given capacity.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to extend.</param>
    /// <param name="maxHistoryPerCheck">Maximum records retained per check name.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddHealthCheckHistory(
        this IServiceCollection services,
        int maxHistoryPerCheck = 10)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton(new HealthCheckHistory(maxHistoryPerCheck));
        return services;
    }

    /// <summary>
    /// Maps the <c>/health/extended</c> endpoint that returns a JSON array of the
    /// last <em>N</em> <see cref="HealthCheckRecord"/> results per check name.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to extend.</param>
    /// <returns>The <see cref="RouteHandlerBuilder"/> for the mapped endpoint.</returns>
    public static RouteHandlerBuilder MapExtendedHealthEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        return endpoints.MapGet("/health/extended", (HealthCheckHistory history) =>
        {
            var all = history.GetAll();
            var payload = all.SelectMany(kvp => kvp.Value).OrderByDescending(r => r.CheckedAt).Select(r => new
            {
                name = r.Name,
                status = r.Status.ToString(),
                checkedAt = r.CheckedAt,
                description = r.Description,
            });

            return Results.Ok(payload);
        });
    }
}
