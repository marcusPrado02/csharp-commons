namespace MarcusPrado.Platform.Redis.Caching;

/// <summary>Configuration options for <see cref="RedisCache"/>.</summary>
public sealed class RedisCacheOptions
{
    /// <summary>Gets or sets the Redis connection string.</summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>Gets or sets an optional key prefix applied to every cache key.</summary>
    public string KeyPrefix { get; set; } = "cache:";

    /// <summary>Gets or sets the default TTL when not specified at call site.</summary>
    public TimeSpan DefaultExpiry { get; set; } = TimeSpan.FromMinutes(30);
}
