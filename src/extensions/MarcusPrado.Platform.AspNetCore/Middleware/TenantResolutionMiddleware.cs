using MarcusPrado.Platform.Abstractions.Context;

namespace MarcusPrado.Platform.AspNetCore.Middleware;

/// <summary>
/// ASP.NET Core middleware that resolves the current tenant and stores it in
/// <see cref="ITenantContext"/>.
///
/// Resolution order (first non-null wins):
///   1. <c>X-Tenant-ID</c> request header.
///   2. <c>tenant_id</c> JWT claim (requires authentication middleware to run first).
///   3. First subdomain segment (e.g. <c>acme</c> from <c>acme.api.example.com</c>).
/// </summary>
public sealed class TenantResolutionMiddleware
{
    /// <summary>Header name used to pass the tenant identifier.</summary>
    public const string TenantIdHeader = "X-Tenant-ID";

    /// <summary>JWT / OAuth2 claim name that carries the tenant identifier.</summary>
    public const string TenantIdClaim = "tenant_id";

    private readonly RequestDelegate _next;

    /// <summary>Initialises the middleware.</summary>
    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    /// <summary>Resolves the tenant and advances the pipeline.</summary>
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        string? tenantId = null;

        // 1. Explicit header (highest priority — useful for internal service calls)
        if (context.Request.Headers.TryGetValue(TenantIdHeader, out var header))
            tenantId = header.FirstOrDefault();

        // 2. JWT claim (works after authentication middleware has populated context.User)
        if (tenantId is null && context.User.Identity?.IsAuthenticated == true)
            tenantId = context.User.FindFirst(TenantIdClaim)?.Value;

        // 3. Subdomain (e.g. https://acme.api.example.com → "acme")
        if (tenantId is null)
        {
            var host = context.Request.Host.Host;
            var parts = host.Split('.');
            if (parts.Length > 2)
                tenantId = parts[0];
        }

        tenantContext.SetTenantId(tenantId);

        await _next(context);
    }
}
