namespace MarcusPrado.Platform.Secrets;

public sealed class SecretCacheOptions
{
    /// <summary>How long secrets are cached. Default: 5 minutes.</summary>
    public TimeSpan Ttl { get; set; } = TimeSpan.FromMinutes(5);
}
