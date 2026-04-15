using System.Diagnostics.Metrics;
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
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddPlatformBusinessMetrics(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton(new Meter("MarcusPrado.Platform.Business", "1.0.0"));
        services.AddSingleton<IBusinessMetrics, OtelBusinessMetrics>();
        return services;
    }
}
