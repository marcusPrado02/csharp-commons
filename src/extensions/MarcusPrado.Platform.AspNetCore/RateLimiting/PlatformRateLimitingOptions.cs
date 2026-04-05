namespace MarcusPrado.Platform.AspNetCore.RateLimiting;

/// <summary>Configuration options for platform HTTP rate limiting.</summary>
public sealed class PlatformRateLimitingOptions
{
    /// <summary>Permits per window for the per-tenant policy (default: 200).</summary>
    public int TenantPermitLimit { get; set; } = 200;

    /// <summary>Window for the per-tenant policy (default: 1 minute).</summary>
    public TimeSpan TenantWindow { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>Permits per window for the per-user policy (default: 60).</summary>
    public int UserPermitLimit { get; set; } = 60;

    /// <summary>Window for the per-user policy (default: 1 minute).</summary>
    public TimeSpan UserWindow { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>Sliding window segment count for the per-user policy (default: 6).</summary>
    public int UserSegmentsPerWindow { get; set; } = 6;

    /// <summary>Permits per window for the per-IP policy (default: 300).</summary>
    public int IpPermitLimit { get; set; } = 300;

    /// <summary>Window for the per-IP policy (default: 1 minute).</summary>
    public TimeSpan IpWindow { get; set; } = TimeSpan.FromMinutes(1);
}
