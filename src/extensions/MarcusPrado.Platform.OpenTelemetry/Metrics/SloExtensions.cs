using MarcusPrado.Platform.Observability.SLO;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.OpenTelemetry.Metrics;

/// <summary>
/// Extension methods for registering SLO/Error Budget tracking services.
/// </summary>
public static class SloExtensions
{
    /// <summary>
    /// Registers <see cref="SloMetricsCollector"/> as a singleton in the service container,
    /// wiring it to the provided <paramref name="slo"/> and <paramref name="snapshotProvider"/>.
    /// </summary>
    /// <param name="services">The service collection to add the SLO services to.</param>
    /// <param name="slo">The service level objective configuration.</param>
    /// <param name="snapshotProvider">A delegate that returns the current <see cref="SloSnapshot"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddPlatformSlo(
        this IServiceCollection services,
        ServiceLevelObjective slo,
        Func<SloSnapshot> snapshotProvider)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(slo);
        ArgumentNullException.ThrowIfNull(snapshotProvider);

        services.AddSingleton(_ => new SloMetricsCollector(slo, snapshotProvider));
        return services;
    }
}
