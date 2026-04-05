namespace MarcusPrado.Platform.Security.Oidc;

public sealed class OidcClientOptions
{
    public string Authority { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string Scope { get; set; } = string.Empty;

    /// <summary>How many seconds before expiry to proactively refresh. Default: 30.</summary>
    public int RefreshBeforeExpirySeconds { get; set; } = 30;
}
