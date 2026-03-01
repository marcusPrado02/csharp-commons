using MarcusPrado.Platform.Abstractions.Context;

namespace MarcusPrado.Platform.AspNetCore.Internal;

/// <summary>
/// Default <see cref="ITenantContext"/> implementation backed by an instance
/// field. Registered as <c>Scoped</c> so it is fresh per HTTP request.
/// </summary>
internal sealed class DefaultTenantContext : ITenantContext
{
    private string? _tenantId;

    /// <inheritdoc />
    public string? TenantId => _tenantId;

    /// <inheritdoc />
    public void SetTenantId(string? tenantId) => _tenantId = tenantId;
}
