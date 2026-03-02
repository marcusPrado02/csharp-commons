using MarcusPrado.Platform.Observability.Metrics;
using MarcusPrado.Platform.OpenTelemetry.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.OpenTelemetry.Metrics;

/// <summary>Extension methods for registering OpenTelemetry business metrics.</summary>
public static class BusinessMetricsExtensions
{
    /// <summary>
    /// Registers <see cref="OtelBusinessMetrics"/> as the singleton
    /// <see cref="IBusinessMetrics"/> implementation.
    /// </summary>
    public static IServiceCollection AddPlatformBusinessMetrics(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IBusinessMetrics, OtelBusinessMetrics>();
        return services;
    }
}
