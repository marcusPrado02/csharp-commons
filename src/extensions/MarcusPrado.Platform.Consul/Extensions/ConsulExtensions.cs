using global::Consul;
using MarcusPrado.Platform.Abstractions.ServiceDiscovery;
using MarcusPrado.Platform.Consul.Options;
using MarcusPrado.Platform.Consul.ServiceDiscovery;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Consul.Extensions;

/// <summary>Extension methods to register Consul service discovery.</summary>
public static class ConsulExtensions
{
    /// <summary>Registers <see cref="IServiceDiscovery"/> backed by Consul.</summary>
    public static IServiceCollection AddPlatformConsul(
        this IServiceCollection services,
        Action<ConsulOptions>? configure = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        var opts = new ConsulOptions();
        configure?.Invoke(opts);

        services.AddSingleton(opts);
        services.AddSingleton<IConsulClient>(_ =>
        {
            var config = new ConsulClientConfiguration { Address = new Uri(opts.Address), Token = opts.Token };
            return new ConsulClient(config);
        });
        services.AddSingleton<IServiceDiscovery, ConsulServiceDiscovery>();

        return services;
    }
}
