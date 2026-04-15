using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.ChaosKit;

/// <summary>
/// Extension methods for registering ChaosKit services with the DI container.
/// </summary>
public static class ChaosExtensions
{
    /// <summary>
    /// Registers <see cref="ChaosConfig"/> and the chaos fault types with the
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">An optional delegate used to configure <see cref="ChaosConfig"/>.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddPlatformChaos(
        this IServiceCollection services,
        Action<ChaosConfig>? configure = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        var config = new ChaosConfig();
        configure?.Invoke(config);

        services.AddSingleton(config);

        return services;
    }
}
