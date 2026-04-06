using MarcusPrado.Platform.Domain.Auditing;

namespace MarcusPrado.Platform.EfCore.Tests.Helpers;

/// <summary>
/// Minimal entity that implements <see cref="IAuditable"/> for testing automatic audit stamping.
/// </summary>
internal sealed class AuditableTestEntity : IAuditable
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    /// <summary>The audit record – filled automatically by <see cref="AppDbContextBase"/>.</summary>
    public AuditRecord Audit { get; set; } = null!;
}
