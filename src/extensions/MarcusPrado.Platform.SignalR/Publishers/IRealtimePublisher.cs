namespace MarcusPrado.Platform.SignalR.Publishers;

/// <summary>Abstracts real-time message dispatch over SignalR.</summary>
public interface IRealtimePublisher
{
    /// <summary>Sends a message to all clients subscribed to a topic.</summary>
    Task PublishAsync<T>(string topic, T payload, CancellationToken cancellationToken = default);

    /// <summary>Sends a message to all clients in a specific tenant group.</summary>
    Task PublishToTenantAsync<T>(
        string tenantId,
        string topic,
        T payload,
        CancellationToken cancellationToken = default
    );

    /// <summary>Sends a message to a specific user.</summary>
    Task PublishToUserAsync<T>(string userId, string topic, T payload, CancellationToken cancellationToken = default);
}
