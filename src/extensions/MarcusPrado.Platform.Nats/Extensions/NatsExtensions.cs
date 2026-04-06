using MarcusPrado.Platform.Nats.Health;

namespace MarcusPrado.Platform.Nats.Extensions;

/// <summary>Extension methods for registering NATS platform services.</summary>
public static class NatsExtensions
{
    /// <summary>
    /// Registers the <see cref="INatsPublisher"/>, <see cref="INatsConsumer"/>,
    /// <see cref="INatsConnection"/>, and a NATS health check with the DI container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">An optional delegate to configure <see cref="NatsOptions"/>.</param>
    /// <returns>The configured <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddPlatformNats(
        this IServiceCollection services,
        Action<NatsOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var opts = new NatsOptions();
        configure?.Invoke(opts);

        services.AddSingleton(opts);

        services.AddSingleton<INatsConnection>(
            _ => new NatsConnection(new NatsOpts
            {
                Url = opts.Url,
                MaxReconnectRetry = opts.MaxReconnectAttempts,
            }));

        services.AddSingleton<INatsPublisher, NatsPublisher>();
        services.AddSingleton<INatsConsumer, NatsConsumer>();

        services.AddHealthChecks()
            .AddCheck<NatsHealthProbe>("nats");

        return services;
    }
}
