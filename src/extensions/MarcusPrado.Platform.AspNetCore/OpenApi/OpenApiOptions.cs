namespace MarcusPrado.Platform.AspNetCore.OpenApi;

/// <summary>
/// Configuration options for the Platform OpenAPI integration.
/// </summary>
public sealed class PlatformOpenApiOptions
{
    /// <summary>Gets or sets the API title shown in the OpenAPI document.</summary>
    public string Title { get; set; } = "Platform API";

    /// <summary>Gets or sets the API version string.</summary>
    public string Version { get; set; } = "v1";

    /// <summary>Gets or sets the API description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether JWT bearer authentication is advertised.</summary>
    public bool EnableJwtAuth { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether API-key authentication is advertised.</summary>
    public bool EnableApiKeyAuth { get; set; }

    /// <summary>Gets or sets the name of the JWT security scheme.</summary>
    public string JwtSchemeName { get; set; } = "Bearer";

    /// <summary>Gets or sets the name of the API-key security scheme.</summary>
    public string ApiKeySchemeName { get; set; } = "ApiKey";

    /// <summary>Gets or sets the header name used for the API key.</summary>
    public string ApiKeyHeaderName { get; set; } = "X-Api-Key";

    /// <summary>Gets or sets a value indicating whether platform context headers are injected into every operation.</summary>
    public bool IncludeContextHeaders { get; set; } = true;
}
