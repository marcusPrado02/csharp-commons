using MarcusPrado.Platform.Abstractions.Context;

namespace MarcusPrado.Platform.AspNetCore.Internal;

/// <summary>
/// Default <see cref="ICorrelationContext"/> implementation backed by instance
/// fields. Registered as <c>Scoped</c> so it is fresh per HTTP request.
/// </summary>
internal sealed class DefaultCorrelationContext : ICorrelationContext
{
    private string _correlationId = string.Empty;
    private string _requestId = string.Empty;

    /// <inheritdoc />
    public string CorrelationId => _correlationId;

    /// <inheritdoc />
    public string RequestId => _requestId;

    /// <inheritdoc />
    public void SetCorrelationId(string correlationId) =>
        _correlationId = correlationId ?? throw new ArgumentNullException(nameof(correlationId));

    /// <inheritdoc />
    public void SetRequestId(string requestId) =>
        _requestId = requestId ?? throw new ArgumentNullException(nameof(requestId));
}
