using System.Net.Http;
using MarcusPrado.Platform.Abstractions.Context;

namespace MarcusPrado.Platform.Http.Handlers;

/// <summary>
/// Propagates the <c>X-Tenant-ID</c> header from the current
/// <see cref="ITenantContext"/> to outgoing HTTP requests.
/// </summary>
public sealed class TenantHeaderHandler : DelegatingHandler
{
    private readonly ITenantContext _tenant;

    /// <summary>Initialises with the ambient tenant context.</summary>
    public TenantHeaderHandler(ITenantContext tenant)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        _tenant = tenant;
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!string.IsNullOrEmpty(_tenant.TenantId))
        {
            request.Headers.TryAddWithoutValidation("X-Tenant-ID", _tenant.TenantId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
