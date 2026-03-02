namespace MarcusPrado.Platform.Http.Clients;

/// <summary>Options for a typed HTTP client registered with the platform factory.</summary>
public sealed class HttpClientOptions
{
    /// <summary>Base address for the HTTP client.</summary>
    public Uri? BaseAddress { get; set; }

    /// <summary>Request timeout (default: 30 seconds).</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Maximum automatic redirects (default: 5).</summary>
    public int MaxAutomaticRedirections { get; set; } = 5;
}
