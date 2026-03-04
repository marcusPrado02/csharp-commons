namespace MarcusPrado.Platform.MultiTenancy.Context;

/// <summary>Carries the resolved tenant identity for the current request scope.</summary>
public sealed class TenantContext
{
    /// <summary>The resolved tenant identifier, or <see langword="null"/> when anonymous.</summary>
    public string? TenantId { get; private set; }

    /// <summary><see langword="true"/> when the tenant has been resolved.</summary>
    public bool IsResolved => TenantId is not null;

    /// <summary>Sets the tenant ID for this context.</summary>
    public void SetTenant(string tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        TenantId = tenantId;
    }
}
