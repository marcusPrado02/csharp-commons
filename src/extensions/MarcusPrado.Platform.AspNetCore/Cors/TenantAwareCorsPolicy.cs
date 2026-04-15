using MarcusPrado.Platform.Abstractions.Context;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace MarcusPrado.Platform.AspNetCore.Cors;

/// <summary>
/// An <see cref="ICorsPolicyProvider"/> that resolves CORS policies based on
/// the current tenant's configured origins.
/// </summary>
public sealed class TenantAwareCorsPolicy : ICorsPolicyProvider
{
    private readonly ICorsPolicyProvider _inner;
    private readonly PlatformCorsOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="TenantAwareCorsPolicy"/>.
    /// </summary>
    public TenantAwareCorsPolicy(ICorsPolicyProvider inner, PlatformCorsOptions options)
    {
        _inner = inner;
        _options = options;
    }

    /// <inheritdoc />
    public async Task<CorsPolicy?> GetPolicyAsync(HttpContext context, string? policyName)
    {
        if (policyName != CorsConstants.TenantPolicy)
            return await _inner.GetPolicyAsync(context, policyName);

        var tenantCtx = context.RequestServices.GetService<ITenantContext>();
        var tenantId = tenantCtx?.TenantId;

        if (tenantId is null || !_options.TenantOrigins.TryGetValue(tenantId, out var origins))
            return new CorsPolicyBuilder().Build(); // deny all

        return new CorsPolicyBuilder().WithOrigins(origins).AllowAnyMethod().AllowAnyHeader().Build();
    }
}
