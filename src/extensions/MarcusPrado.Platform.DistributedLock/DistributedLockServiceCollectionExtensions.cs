using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace MarcusPrado.Platform.DistributedLock;

/// <summary>
/// Extension methods for registering distributed lock services.
/// </summary>
public static class DistributedLockServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="RedisDistributedLock"/> as the <see cref="IDistributedLock"/>
    /// implementation, resolved from an already-registered <see cref="IConnectionMultiplexer"/>.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <remarks>
    /// Requires that <see cref="IConnectionMultiplexer"/> is already registered in the DI container
    /// (e.g. via <c>AddStackExchangeRedisCache</c> or a custom registration).
    /// </remarks>
    public static IServiceCollection AddPlatformDistributedLock(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IDistributedLock>(sp =>
        {
            var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
            return new RedisDistributedLock(multiplexer.GetDatabase());
        });

        return services;
    }
}
