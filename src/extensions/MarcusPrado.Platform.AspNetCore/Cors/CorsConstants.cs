namespace MarcusPrado.Platform.AspNetCore.Cors;

/// <summary>
/// Well-known CORS policy names used by the platform.
/// </summary>
public static class CorsConstants
{
    /// <summary>The default platform CORS policy name.</summary>
    public const string DefaultPolicy = "PlatformDefault";

    /// <summary>The tenant-aware CORS policy name.</summary>
    public const string TenantPolicy = "PlatformTenant";
}
