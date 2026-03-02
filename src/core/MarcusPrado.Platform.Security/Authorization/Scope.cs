namespace MarcusPrado.Platform.Security.Authorization;

/// <summary>
/// Represents an OAuth2 scope that controls access to a resource server.
/// Scope values follow the <c>resource:action</c> or simple-name convention
/// (e.g. <c>api:read</c>, <c>openid</c>).
/// </summary>
public sealed record Scope
{
    /// <summary>Gets the canonical string value of this scope.</summary>
    public string Value { get; }

    /// <summary>Initialises a new scope with the given value.</summary>
    public Scope(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    /// <summary>Implicitly creates a <see cref="Scope"/> from a string.</summary>
    public static implicit operator Scope(string value) => new(value);

    /// <summary>Returns the scope value.</summary>
    public override string ToString() => Value;
}
