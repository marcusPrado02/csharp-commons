namespace MarcusPrado.Platform.Runtime.Configuration;

/// <summary>
/// Type-safe abstraction for reading application configuration.
/// Follows the 12-Factor App principle — env vars override file-based config.
/// </summary>
public interface IAppConfiguration
{
    /// <summary>
    /// Returns the value bound to <paramref name="key"/>, or <see langword="null"/>
    /// when absent from both environment variables and the config files.
    /// </summary>
    T? Get<T>(ConfigurationKey<T> key);

    /// <summary>
    /// Binds a configuration section as <typeparamref name="TSection"/>.
    /// </summary>
    TSection GetSection<TSection>(string sectionKey)
        where TSection : class;
}
