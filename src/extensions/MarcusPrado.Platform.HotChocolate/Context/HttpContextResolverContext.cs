using System.Security.Claims;
using MarcusPrado.Platform.Abstractions.GraphQL;
using Microsoft.AspNetCore.Http;

namespace MarcusPrado.Platform.HotChocolate.Context;

/// <summary>
/// Implements <see cref="IPlatformResolverContext"/> by reading values from
/// the current <see cref="HttpContext"/> via <see cref="IHttpContextAccessor"/>.
/// </summary>
public sealed class HttpContextResolverContext : IPlatformResolverContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>Initializes a new instance of <see cref="HttpContextResolverContext"/>.</summary>
    public HttpContextResolverContext(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpContext? Context => _httpContextAccessor.HttpContext;

    /// <inheritdoc />
    public string? TenantId =>
        Context?.User.FindFirstValue("tenant_id")
        ?? Context?.Request.Headers["X-Tenant-Id"].FirstOrDefault();

    /// <inheritdoc />
    public string? UserId =>
        Context?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? Context?.User.FindFirstValue("sub");

    /// <inheritdoc />
    public string? CorrelationId =>
        Context?.Request.Headers["X-Correlation-Id"].FirstOrDefault()
        ?? Context?.Request.Headers["X-Request-Id"].FirstOrDefault();

    /// <inheritdoc />
    public bool IsAuthenticated =>
        Context?.User.Identity?.IsAuthenticated ?? false;
}
