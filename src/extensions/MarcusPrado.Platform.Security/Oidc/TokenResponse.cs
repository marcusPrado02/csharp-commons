namespace MarcusPrado.Platform.Security.Oidc;

public sealed record TokenResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    DateTimeOffset IssuedAt)
{
    public DateTimeOffset ExpiresAt => IssuedAt.AddSeconds(ExpiresIn);
    public bool IsExpired(int bufferSeconds = 0) =>
        DateTimeOffset.UtcNow >= ExpiresAt.AddSeconds(-bufferSeconds);
}
