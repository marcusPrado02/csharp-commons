namespace MarcusPrado.Platform.Domain.Identity;

/// <summary>
/// Strongly-typed identifier for a <c>User</c> / principal.
/// Backed by a <see cref="Guid"/>.
/// </summary>
public sealed record UserId(Guid Value) : EntityId<Guid>(Value)
{
    /// <summary>Creates a new random <see cref="UserId"/>.</summary>
    public static UserId New() => new(Guid.NewGuid());

    /// <summary>Converts a raw <see cref="Guid"/> to a <see cref="UserId"/> implicitly.</summary>
    public static implicit operator UserId(Guid value) => new(value);

    /// <summary>Extracts the underlying <see cref="Guid"/> from a <see cref="UserId"/>.</summary>
    public static implicit operator Guid(UserId id) => id.Value;
}
