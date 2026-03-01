namespace MarcusPrado.Platform.Domain.Identity;

/// <summary>
/// Non-generic marker base for all strongly-typed entity identifiers.
/// Use this as a constraint in generic repository signatures:
/// <c>IRepository&lt;TAgg, TId&gt; where TId : EntityId</c>.
/// </summary>
public abstract record EntityId
{
    /// <summary>Returns the identifier value as a string (for persistence / logging).</summary>
    public abstract override string ToString();
}

/// <summary>
/// Generic base for strongly-typed identifiers backed by a primitive value.
/// Derive a sealed record per aggregate to get compile-time type safety and
/// prevent accidentally passing a <c>UserId</c> where a <c>TenantId</c> is expected.
/// </summary>
/// <typeparam name="TValue">Underlying primitive type (usually <see cref="Guid"/> or <see cref="string"/>).</typeparam>
public abstract record EntityId<TValue>(TValue Value) : EntityId
    where TValue : notnull
{
    /// <inheritdoc/>
    public sealed override string ToString() => Value.ToString()!;
}
