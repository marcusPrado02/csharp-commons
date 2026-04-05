namespace MarcusPrado.Platform.Secrets;

public sealed class CachedSecretProvider : ISecretProvider
{
    private readonly ISecretProvider _inner;
    private readonly IMemoryCache _cache;
    private readonly SecretCacheOptions _options;

    private static string CacheKey(string name) => $"secret:{name}";

    public CachedSecretProvider(ISecretProvider inner, IMemoryCache cache, SecretCacheOptions options)
    {
        _inner = inner;
        _cache = cache;
        _options = options;
    }

    public async Task<string?> GetSecretAsync(string name, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey(name), out string? cached))
            return cached;

        var value = await _inner.GetSecretAsync(name, cancellationToken);
        if (value is not null)
            _cache.Set(CacheKey(name), value, _options.Ttl);

        return value;
    }

    public Task InvalidateCacheAsync(string name, CancellationToken cancellationToken = default)
    {
        _cache.Remove(CacheKey(name));
        return Task.CompletedTask;
    }
}
