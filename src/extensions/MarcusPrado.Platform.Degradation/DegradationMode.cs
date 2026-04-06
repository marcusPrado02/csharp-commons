namespace MarcusPrado.Platform.Degradation;

/// <summary>
/// Represents the operational degradation mode of the application.
/// </summary>
public enum DegradationMode
{
    /// <summary>
    /// Normal operation — all requests are processed without restriction.
    /// </summary>
    None,

    /// <summary>
    /// Partially degraded — some non-critical features may be unavailable.
    /// An <c>X-Degradation-Mode: PartiallyDegraded</c> header is added to responses.
    /// </summary>
    PartiallyDegraded,

    /// <summary>
    /// Read-only mode — write operations (POST, PUT, DELETE, PATCH) are rejected
    /// with HTTP 405 Method Not Allowed.
    /// </summary>
    ReadOnly,

    /// <summary>
    /// Maintenance mode — all requests are rejected with HTTP 503 Service Unavailable.
    /// </summary>
    Maintenance,
}
