namespace MarcusPrado.Platform.Security.Oidc;

public sealed class OidcClientService : IOidcClientService, IDisposable
{
    private readonly HttpClient _http;
    private readonly OidcClientOptions _options;
    private readonly ILogger<OidcClientService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private TokenResponse? _cached;

    public OidcClientService(HttpClient http, IOptions<OidcClientOptions> options, ILogger<OidcClientService> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_cached is not null && !_cached.IsExpired(_options.RefreshBeforeExpirySeconds))
            return _cached.AccessToken;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (_cached is not null && !_cached.IsExpired(_options.RefreshBeforeExpirySeconds))
                return _cached.AccessToken;

            _cached = await FetchTokenAsync(cancellationToken);
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("OIDC token refreshed, expires at {ExpiresAt}", _cached.ExpiresAt);
            return _cached.AccessToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<TokenResponse> FetchTokenAsync(CancellationToken cancellationToken)
    {
        var tokenEndpoint = _options.Authority.TrimEnd('/') + "/connect/token";
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["scope"] = _options.Scope,
        };

        using var response = await _http.PostAsync(tokenEndpoint, new FormUrlEncodedContent(form), cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(cancellationToken: cancellationToken);
        return new TokenResponse(
            AccessToken: json.GetProperty("access_token").GetString()!,
            TokenType: json.GetProperty("token_type").GetString()!,
            ExpiresIn: json.GetProperty("expires_in").GetInt32(),
            IssuedAt: DateTimeOffset.UtcNow);
    }

    public void Dispose() => _lock.Dispose();
}
