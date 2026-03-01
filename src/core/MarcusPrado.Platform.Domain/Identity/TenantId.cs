namespace MarcusPrado.Platform.Domain.Identity;

/// <summary>
/// Strongly-typed identifier for a <c>Tenant</c> aggregate.
/// Backed by a <see cref="Guid"/>.
/// </summary>
public sealed record TenantId(Guid Value) : EntityId<Guid>(Value)
{
    /// <summary>Creates a new random <see cref="TenantId"/>.</summary>
    public static TenantId New() => new(Guid.NewGuid());

    /// <summary>Converts a raw <see cref="Guid"/> to a <see cref="TenantId"/> implicitly.</summary>
    public static implicit operator TenantId(Guid value) => new(value);

    /// <summary>Extracts the underlying <see cref="Guid"/> from a <see cref="TenantId"/>.</summary>
    public static implicit operator Guid(TenantId id) => id.Value;
}
