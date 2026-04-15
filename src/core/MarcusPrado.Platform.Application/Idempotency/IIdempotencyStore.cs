namespace MarcusPrado.Platform.Application.Idempotency;

/// <summary>Persists and retrieves idempotent command responses.</summary>
public interface IIdempotencyStore
{
    /// <summary>
    /// Attempts to retrieve a previously cached response.
    /// </summary>
    /// <returns>
    /// <c>(true, serializedResponse)</c> when a cached entry exists;
    /// <c>(false, null)</c> otherwise.
    /// </returns>
    Task<(bool Found, string? SerializedResponse)> TryGetAsync(
        string key,
        CancellationToken cancellationToken = default
    );

    /// <summary>Stores a serialized response under the given key with a TTL.</summary>
    Task SetAsync(
        string key,
        string serializedResponse,
        TimeSpan timeToLive,
        CancellationToken cancellationToken = default
    );
}
