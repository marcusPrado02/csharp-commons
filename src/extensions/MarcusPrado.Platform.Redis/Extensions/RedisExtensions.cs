using MarcusPrado.Platform.OutboxInbox.Idempotency;
using MarcusPrado.Platform.RateLimiting.Quotas;
using MarcusPrado.Platform.Redis.Caching;
using MarcusPrado.Platform.Redis.Idempotency;
using MarcusPrado.Platform.Redis.RateLimit;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace MarcusPrado.Platform.Redis.Extensions;

/// <summary>Extension methods for registering Redis platform services.</summary>
public static class RedisExtensions
{
    /// <summary>
    /// Registers <see cref="ICache"/>, <see cref="IQuotaStore"/>, and
    /// <see cref="IIdempotencyStore"/> backed by Redis.
    /// </summary>
    public static IServiceCollection AddPlatformRedis(
        this IServiceCollection services,
        Action<RedisCacheOptions>? configure = null)
    {
        var opts = new RedisCacheOptions();
        configure?.Invoke(opts);

        services.AddSingleton<IConnectionMultiplexer>(
            _ => ConnectionMultiplexer.Connect(opts.ConnectionString));

        services.AddSingleton(opts);
        services.AddSingleton<ICache, RedisCache>();
        services.AddSingleton<IQuotaStore, RedisQuotaStore>();
        services.AddSingleton<IIdempotencyStore, RedisIdempotencyStore>();

        return services;
    }
}
