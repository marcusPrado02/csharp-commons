using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MarcusPrado.Platform.Configuration;

/// <summary>
/// Extension methods for registering hot-reload configuration services.
/// </summary>
public static class HotReloadExtensions
{
    /// <summary>
    /// Registers <see cref="IOptionsHotReload{T}"/>, <see cref="OptionsHotReload{T}"/>,
    /// <see cref="ConfigurationValidator{T}"/>, and <see cref="ConfigurationChangeLogger"/>
    /// into the service collection.
    /// </summary>
    /// <typeparam name="T">The options type to register hot-reload support for.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddPlatformOptionsHotReload<T>(this IServiceCollection services)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<ConfigurationChangeLogger>();
        services.TryAddSingleton<ConfigurationValidator<T>>();
        services.TryAddSingleton<IOptionsHotReload<T>, OptionsHotReload<T>>();

        return services;
    }
}
