namespace MarcusPrado.Platform.SignalR.Publishers;

/// <summary>
/// <see cref="IRealtimePublisher"/> implementation backed by a strongly-typed
/// <see cref="IHubContext{THub}"/>.
/// </summary>
public sealed class HubRealtimePublisher<THub> : IRealtimePublisher
    where THub : Hub
{
    private readonly IHubContext<THub> _hubContext;

    /// <summary>Initializes a new instance of <see cref="HubRealtimePublisher{THub}"/>.</summary>
    public HubRealtimePublisher(IHubContext<THub> hubContext) => _hubContext = hubContext;

    /// <inheritdoc />
    public Task PublishAsync<T>(string topic, T payload, CancellationToken cancellationToken = default) =>
        _hubContext.Clients.All.SendAsync(topic, payload, cancellationToken);

    /// <inheritdoc />
    public Task PublishToTenantAsync<T>(
        string tenantId,
        string topic,
        T payload,
        CancellationToken cancellationToken = default
    ) => _hubContext.Clients.Group($"tenant:{tenantId}").SendAsync(topic, payload, cancellationToken);

    /// <inheritdoc />
    public Task PublishToUserAsync<T>(
        string userId,
        string topic,
        T payload,
        CancellationToken cancellationToken = default
    ) => _hubContext.Clients.User(userId).SendAsync(topic, payload, cancellationToken);
}
