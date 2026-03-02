namespace MarcusPrado.Platform.Security.Authorization;

/// <summary>
/// Represents a named permission that may be granted to a principal.
/// Permissions follow the <c>action:resource</c> naming convention
/// (e.g. <c>read:users</c>, <c>write:orders</c>).
/// </summary>
public sealed record Permission
{
    /// <summary>Gets the canonical string value of this permission.</summary>
    public string Value { get; }

    /// <summary>Initialises a new permission with the given value.</summary>
    public Permission(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    /// <summary>Implicitly creates a <see cref="Permission"/> from a string.</summary>
    public static implicit operator Permission(string value) => new(value);

    /// <summary>Returns the permission value.</summary>
    public override string ToString() => Value;
}
