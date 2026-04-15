namespace MarcusPrado.Platform.Security.Encryption;

public static class EncryptionExtensions
{
    public static IServiceCollection AddPlatformEncryption(this IServiceCollection services, byte[] key)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IDataEncryption>(_ => new AesGcmEncryption(key));
        return services;
    }

    public static IServiceCollection AddPlatformEncryption(
        this IServiceCollection services,
        IReadOnlyDictionary<int, byte[]> keys,
        int currentVersion
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IDataEncryption>(new KeyRotationService(keys, currentVersion));
        return services;
    }
}
