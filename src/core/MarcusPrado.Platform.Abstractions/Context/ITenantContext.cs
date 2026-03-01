namespace MarcusPrado.Platform.Abstractions.Context;

/// <summary>
/// Holds the resolved tenant identifier for the current request.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Gets the current tenant identifier, or <c>null</c> for single-tenant
    /// deployments.
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// Sets the tenant ID. Called once during tenant resolution in the middleware
    /// pipeline.
    /// </summary>
    void SetTenantId(string? tenantId);
}
