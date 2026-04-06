namespace MarcusPrado.Platform.EfCore.Tests.Helpers;

/// <summary>
/// Minimal entity with a <c>TenantId</c> property so the tenant-filter decorator
/// can apply its global query filter during tests.
/// </summary>
internal sealed class TenantTestEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;
}
