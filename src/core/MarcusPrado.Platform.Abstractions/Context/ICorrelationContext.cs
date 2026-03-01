namespace MarcusPrado.Platform.Abstractions.Context;

/// <summary>
/// Holds correlation identifiers for the current request, enabling distributed
/// tracing across service boundaries.
/// </summary>
public interface ICorrelationContext
{
    /// <summary>
    /// Gets the correlation ID that ties related requests together across multiple
    /// services (typically propagated via the <c>X-Correlation-ID</c> header).
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// Gets the unique identifier for this specific HTTP request
    /// (typically propagated via the <c>X-Request-ID</c> header).
    /// </summary>
    string RequestId { get; }

    /// <summary>
    /// Sets the correlation ID. Called once early in the middleware pipeline.
    /// </summary>
    void SetCorrelationId(string correlationId);

    /// <summary>
    /// Sets the request ID. Called once early in the middleware pipeline.
    /// </summary>
    void SetRequestId(string requestId);
}
