using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace MarcusPrado.Platform.Redis.Stampede;

/// <summary>
/// Extension methods for registering cache stampede prevention services in the DI container.
/// </summary>
public static class CacheStampedeExtensions
{
    /// <summary>
    /// Registers <see cref="StampedeProtectedCache"/> as a singleton in the DI container.
    /// Requires a prior registration of <see cref="IDistributedCache"/> (e.g. via
    /// <c>AddStackExchangeRedisCache</c> or <c>AddDistributedMemoryCache</c>).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddStampedeProtectedCache(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<StampedeProtectedCache>();
        return services;
    }

    /// <summary>
    /// Registers a single cache warmup action and the <see cref="CacheWarmupService"/>
    /// <see cref="IHostedService"/> (if not already registered).
    /// Multiple calls to this method accumulate warmup actions.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="warmupAction">
    /// An async delegate that pre-populates the cache using the provided
    /// <see cref="IDistributedCache"/> on application startup.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddCacheWarmup(
        this IServiceCollection services,
        Func<IDistributedCache, CancellationToken, Task> warmupAction
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(warmupAction);

        // Each call adds one warmup action; all are resolved as IEnumerable<...>
        services.AddSingleton(warmupAction);

        // Register the hosted service only once
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, CacheWarmupService>());

        return services;
    }
}
