using MarcusPrado.Platform.SignalR.Publishers;

namespace MarcusPrado.Platform.SignalR;

/// <summary>Extension methods for registering Platform SignalR services.</summary>
public static class SignalRExtensions
{
    /// <summary>Registers SignalR services + <see cref="HubRealtimePublisher{THub}"/> for the specified hub type.</summary>
    public static IServiceCollection AddPlatformSignalR<THub>(
        this IServiceCollection services,
        Action<HubOptions>? configure = null
    )
        where THub : Hub
    {
        var builder = services.AddSignalR();
        if (configure is not null)
            builder.AddHubOptions<THub>(configure);

        services.AddSingleton<IRealtimePublisher, HubRealtimePublisher<THub>>();
        return services;
    }
}
