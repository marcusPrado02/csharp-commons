namespace MarcusPrado.Platform.AspNetCore.IpFiltering;

/// <summary>
/// Configuration options for the IP filter middleware.
/// </summary>
public sealed class IpFilterOptions
{
    /// <summary>
    /// If non-empty, only requests from IPs matching any entry are allowed.
    /// All other IPs receive a 403 response. Entries may be plain IPs or CIDR ranges.
    /// </summary>
    public IList<string> Whitelist { get; set; } = new List<string>();

    /// <summary>
    /// IPs matching any entry are blocked with a 403 response.
    /// Blacklist is checked before whitelist. Entries may be plain IPs or CIDR ranges.
    /// </summary>
    public IList<string> Blacklist { get; set; } = new List<string>();

    /// <summary>
    /// When <c>true</c>, the client IP is resolved from <c>X-Forwarded-For</c> first,
    /// then <c>X-Real-IP</c>, then <c>RemoteIpAddress</c>.
    /// When <c>false</c>, only <c>RemoteIpAddress</c> is used.
    /// </summary>
    public bool TrustForwardedFor { get; set; }
}
