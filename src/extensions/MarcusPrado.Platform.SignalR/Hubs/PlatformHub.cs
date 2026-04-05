namespace MarcusPrado.Platform.SignalR.Hubs;

/// <summary>
/// Base SignalR hub that propagates correlation ID and tenant ID into
/// the hub context and enforces JWT authentication.
/// </summary>
public abstract class PlatformHub<T> : Hub<T>
    where T : class
{
    private readonly ICorrelationContext _correlation;
    private readonly ITenantContext _tenant;

    /// <summary>Initializes a new instance of <see cref="PlatformHub{T}"/>.</summary>
    protected PlatformHub(ICorrelationContext correlation, ITenantContext tenant)
    {
        _correlation = correlation;
        _tenant = tenant;
    }

    /// <summary>Tenant ID from the connected client's claims or query string.</summary>
    protected string? TenantId => _tenant.TenantId
        ?? Context.User?.FindFirst("tenant_id")?.Value
        ?? Context.GetHttpContext()?.Request.Query["tenantId"].FirstOrDefault();

    /// <summary>Correlation ID for the current hub invocation.</summary>
    protected string CorrelationId => _correlation.CorrelationId;

    /// <inheritdoc />
    public override async Task OnConnectedAsync()
    {
        // Join a tenant-specific group for isolation
        if (TenantId is not null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant:{TenantId}");

        await base.OnConnectedAsync();
    }

    /// <inheritdoc />
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (TenantId is not null)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tenant:{TenantId}");

        await base.OnDisconnectedAsync(exception);
    }
}
