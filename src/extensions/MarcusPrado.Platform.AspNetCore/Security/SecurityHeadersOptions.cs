namespace MarcusPrado.Platform.AspNetCore.Security;

/// <summary>
/// Configuration options for the platform security-headers middleware.
/// All headers are enabled by default and can be disabled individually.
/// </summary>
public sealed class SecurityHeadersOptions
{
    /// <summary>
    /// When <c>true</c>, emits <c>Content-Security-Policy</c> with the value of
    /// <see cref="ContentSecurityPolicy"/> unless the header is already present.
    /// Default: <c>true</c>.
    /// </summary>
    public bool EnableContentSecurityPolicy { get; set; } = true;

    /// <summary>
    /// The value of the <c>Content-Security-Policy</c> header.
    /// Default: <c>"default-src 'self'"</c>.
    /// </summary>
    public string ContentSecurityPolicy { get; set; } = "default-src 'self'";

    /// <summary>
    /// When <c>true</c> and the environment is not Development, <c>UsePlatformHsts()</c>
    /// activates the built-in HSTS middleware.  Default: <c>true</c>.
    /// </summary>
    public bool EnableHsts { get; set; } = true;

    /// <summary>
    /// When <c>true</c>, emits <c>X-Frame-Options: DENY</c>.
    /// Default: <c>true</c>.
    /// </summary>
    public bool EnableXFrameOptions { get; set; } = true;

    /// <summary>
    /// When <c>true</c>, emits <c>X-Content-Type-Options: nosniff</c>.
    /// Default: <c>true</c>.
    /// </summary>
    public bool EnableXContentTypeOptions { get; set; } = true;

    /// <summary>
    /// When <c>true</c>, emits <c>Referrer-Policy</c> with the value of
    /// <see cref="ReferrerPolicy"/>.  Default: <c>true</c>.
    /// </summary>
    public bool EnableReferrerPolicy { get; set; } = true;

    /// <summary>
    /// The value of the <c>Referrer-Policy</c> header.
    /// Default: <c>"no-referrer"</c>.
    /// </summary>
    public string ReferrerPolicy { get; set; } = "no-referrer";
}
