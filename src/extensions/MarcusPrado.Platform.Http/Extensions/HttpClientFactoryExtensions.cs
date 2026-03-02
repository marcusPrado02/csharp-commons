using System.Net.Http;
using MarcusPrado.Platform.Http.Clients;
using MarcusPrado.Platform.Http.Handlers;

namespace MarcusPrado.Platform.Http.Extensions;

/// <summary>
/// Extension methods for registering typed HTTP clients with the platform.
/// </summary>
public static class HttpClientFactoryExtensions
{
    /// <summary>
    /// Registers typed client <typeparamref name="TClient"/> with correlation
    /// and tenant header propagation applied via delegating handlers.
    /// </summary>
    /// <typeparam name="TClient">
    /// The typed client to register; must derive from <see cref="TypedHttpClient"/>.
    /// </typeparam>
    public static IServiceCollection AddPlatformHttpClient<TClient>(
        this IServiceCollection services,
        Action<HttpClientOptions>? configure = null)
        where TClient : TypedHttpClient
    {
        ArgumentNullException.ThrowIfNull(services);

        var opts = new HttpClientOptions();
        configure?.Invoke(opts);

        services.AddTransient<CorrelationHeaderHandler>();
        services.AddTransient<TenantHeaderHandler>();

        services
            .AddHttpClient<TClient>(client =>
            {
                if (opts.BaseAddress is not null)
                {
                    client.BaseAddress = opts.BaseAddress;
                }

                client.Timeout = opts.Timeout;
            })
            .AddHttpMessageHandler<CorrelationHeaderHandler>()
            .AddHttpMessageHandler<TenantHeaderHandler>();

        return services;
    }
}
