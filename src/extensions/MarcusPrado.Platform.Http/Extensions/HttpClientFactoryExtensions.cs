using System.Net.Http;
using MarcusPrado.Platform.Http.Clients;
using MarcusPrado.Platform.Http.Handlers;
using Microsoft.AspNetCore.Http;

namespace MarcusPrado.Platform.Http.Extensions;

/// <summary>
/// Extension methods for registering typed HTTP clients with the platform.
/// </summary>
public static class HttpClientFactoryExtensions
{
    /// <summary>
    /// Registers typed client <typeparamref name="TClient"/> with correlation,
    /// tenant, and auth-token header propagation applied via delegating handlers,
    /// plus standard resilience (retry, circuit-breaker, timeout).
    /// </summary>
    /// <typeparam name="TClient">
    /// The typed client to register; must derive from <see cref="TypedHttpClient"/>.
    /// </typeparam>
    public static IServiceCollection AddPlatformHttpClient<TClient>(
        this IServiceCollection services,
        Action<HttpClientOptions>? configure = null
    )
        where TClient : TypedHttpClient
    {
        ArgumentNullException.ThrowIfNull(services);

        var opts = new HttpClientOptions();
        configure?.Invoke(opts);

        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationHeaderHandler>();
        services.AddTransient<TenantHeaderHandler>();
        services.AddTransient<AuthTokenHandler>();

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
            .AddHttpMessageHandler<TenantHeaderHandler>()
            .AddHttpMessageHandler<AuthTokenHandler>()
            .AddStandardResilienceHandler();

        return services;
    }
}
