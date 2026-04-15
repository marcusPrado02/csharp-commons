using System.Net.Http;
using MarcusPrado.Platform.Abstractions.Context;

namespace MarcusPrado.Platform.Http.Handlers;

/// <summary>
/// Propagates <c>X-Correlation-ID</c> and <c>X-Request-ID</c> headers from
/// the current <see cref="ICorrelationContext"/> to outgoing HTTP requests.
/// </summary>
public sealed class CorrelationHeaderHandler : DelegatingHandler
{
    private readonly ICorrelationContext _correlation;

    /// <summary>Initialises with the ambient correlation context.</summary>
    public CorrelationHeaderHandler(ICorrelationContext correlation)
    {
        ArgumentNullException.ThrowIfNull(correlation);
        _correlation = correlation;
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!string.IsNullOrEmpty(_correlation.CorrelationId))
        {
            request.Headers.TryAddWithoutValidation("X-Correlation-ID", _correlation.CorrelationId);
        }

        if (!string.IsNullOrEmpty(_correlation.RequestId))
        {
            request.Headers.TryAddWithoutValidation("X-Request-ID", _correlation.RequestId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
