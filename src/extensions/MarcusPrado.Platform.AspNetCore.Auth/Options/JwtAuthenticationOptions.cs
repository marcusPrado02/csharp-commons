namespace MarcusPrado.Platform.AspNetCore.Auth.Options;

/// <summary>
/// Configuration options for the platform's JWT authentication handler.
/// Bind this class from <c>appsettings.json</c> or configure it inline via
/// <c>AddPlatformAuth</c>.
/// </summary>
public sealed class JwtAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// Gets or sets the token issuer that will be validated against the
    /// <c>iss</c> JWT claim.  Leave empty to skip issuer validation.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the intended audience validated against the <c>aud</c>
    /// JWT claim.  Leave empty to skip audience validation.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HMAC-SHA256 signing key used to verify the token
    /// signature.  Must be at least 16 characters.
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the token lifetime (the
    /// <c>exp</c> / <c>nbf</c> claims) should be validated.
    /// Defaults to <c>true</c>. Set to <c>false</c> only in tests.
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;
}
