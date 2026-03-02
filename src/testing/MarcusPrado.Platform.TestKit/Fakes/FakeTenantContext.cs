using MarcusPrado.Platform.Abstractions.Context;

namespace MarcusPrado.Platform.TestKit.Fakes;

/// <summary>
/// Configurable <see cref="ITenantContext"/> for use in tests.
/// Set <see cref="TenantId"/> directly before invoking system-under-test code.
/// </summary>
public sealed class FakeTenantContext : ITenantContext
{
    /// <inheritdoc/>
    public string? TenantId { get; private set; }

    /// <summary>Creates a context pre-configured with the given <paramref name="tenantId"/>.</summary>
    public static FakeTenantContext For(string tenantId) => new() { TenantId = tenantId };

    /// <inheritdoc/>
    public void SetTenantId(string? tenantId) => TenantId = tenantId;
}
