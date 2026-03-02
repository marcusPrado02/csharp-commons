namespace MarcusPrado.Platform.Redis.Caching;

/// <summary>Generic key-value cache abstraction with optional TTL support.</summary>
public interface ICache
{
    /// <summary>Returns the cached value for <paramref name="key"/>, or <c>null</c> if absent.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        where T : class;

    /// <summary>Stores <paramref name="value"/> under <paramref name="key"/> with optional TTL.</summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
        where T : class;

    /// <summary>Removes the entry for <paramref name="key"/>.</summary>
    Task RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>Returns <c>true</c> when the key exists and has not expired.</summary>
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);
}
