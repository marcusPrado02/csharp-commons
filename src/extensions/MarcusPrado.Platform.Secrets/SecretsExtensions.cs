namespace MarcusPrado.Platform.Secrets;

public static class SecretsExtensions
{
    public static IServiceCollection AddPlatformSecrets(
        this IServiceCollection services,
        Action<SecretCacheOptions>? configure = null
    )
    {
        var cacheOpts = new SecretCacheOptions();
        configure?.Invoke(cacheOpts);
        services.AddSingleton(cacheOpts);
        services.AddMemoryCache();
        return services;
    }

    public static IServiceCollection AddInMemorySecretProvider(
        this IServiceCollection services,
        IReadOnlyDictionary<string, string>? secrets = null
    )
    {
        services.AddSingleton<ISecretProvider>(new InMemorySecretProvider(secrets));
        return services;
    }

    public static IServiceCollection AddEnvironmentSecretProvider(this IServiceCollection services)
    {
        services.AddSingleton<ISecretProvider, EnvironmentSecretProvider>();
        return services;
    }

    /// <summary>
    /// Registers a <see cref="CachedSecretProvider"/> that wraps an <see cref="InMemorySecretProvider"/>.
    /// Call <see cref="AddPlatformSecrets"/> first to register cache options and memory cache.
    /// </summary>
    public static IServiceCollection AddCachedInMemorySecretProvider(
        this IServiceCollection services,
        IReadOnlyDictionary<string, string>? secrets = null
    )
    {
        var inner = new InMemorySecretProvider(secrets);
        services.AddSingleton(inner);
        services.AddSingleton<ISecretProvider>(sp => new CachedSecretProvider(
            inner,
            sp.GetRequiredService<IMemoryCache>(),
            sp.GetRequiredService<SecretCacheOptions>()
        ));
        return services;
    }
}
