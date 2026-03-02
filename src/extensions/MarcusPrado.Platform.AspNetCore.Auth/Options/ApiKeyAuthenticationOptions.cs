namespace MarcusPrado.Platform.AspNetCore.Auth.Options;

/// <summary>
/// Configuration options for the platform's API-key authentication handler.
/// </summary>
public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>HTTP header name used to pass the API key.</summary>
    public const string DefaultHeaderName = "X-Api-Key";

    /// <summary>
    /// Gets or sets the HTTP header name from which the API key is extracted.
    /// Defaults to <c>X-Api-Key</c>.
    /// </summary>
    public string HeaderName { get; set; } = DefaultHeaderName;

    /// <summary>
    /// Gets or sets the set of valid API keys.  Keys are compared in
    /// a case-sensitive manner.
    /// </summary>
    public IReadOnlyCollection<string> ValidKeys { get; set; } = [];
}
