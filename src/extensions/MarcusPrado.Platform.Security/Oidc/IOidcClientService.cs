namespace MarcusPrado.Platform.Security.Oidc;

public interface IOidcClientService
{
    /// <summary>Returns a valid access token, fetching or refreshing as needed.</summary>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
