using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace MarcusPrado.Platform.Http.Handlers;

/// <summary>
/// Propagates the <c>Authorization: Bearer &lt;token&gt;</c> header from the
/// current inbound HTTP request to outgoing HTTP requests, if not already set.
/// </summary>
public sealed class AuthTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>Initialises with the ambient <see cref="IHttpContextAccessor"/>.</summary>
    /// <param name="httpContextAccessor">Accessor for the current HTTP context.</param>
    public AuthTokenHandler(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!request.Headers.Contains("Authorization"))
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader))
            {
                request.Headers.TryAddWithoutValidation("Authorization", authHeader);
            }
        }

        return base.SendAsync(request, cancellationToken);
    }
}
