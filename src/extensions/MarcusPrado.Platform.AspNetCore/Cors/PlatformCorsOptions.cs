namespace MarcusPrado.Platform.AspNetCore.Cors;

/// <summary>
/// Configuration options for platform CORS policies.
/// </summary>
public sealed class PlatformCorsOptions
{
    /// <summary>Gets or sets the CORS profile to apply.</summary>
    public PlatformCorsProfile Profile { get; set; } = PlatformCorsProfile.DevPermissive;

    /// <summary>Origins allowed in StagingRestricted / ProductionLocked profiles.</summary>
    public string[] AllowedOrigins { get; set; } = [];

    /// <summary>When true, a tenant-aware CORS policy is also registered.</summary>
    public bool EnableTenantAwarePolicy { get; set; }

    /// <summary>Per-tenant allowed origins. Key = tenantId.</summary>
    public Dictionary<string, string[]> TenantOrigins { get; set; } = new();
}
