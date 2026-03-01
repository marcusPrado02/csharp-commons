namespace MarcusPrado.Platform.Domain.SeedWork;

/// <summary>
/// Base class for value objects.
/// Structural equality is derived automatically from the components returned by
/// <see cref="GetEqualityComponents"/>.  Implement that method and add
/// <c>init</c>-only or readonly properties; do NOT add mutable state.
/// </summary>
/// <example>
/// <code>
/// public sealed class Money : ValueObject
/// {
///     public decimal Amount { get; }
///     public string  Currency { get; }
///     public Money(decimal amount, string currency) { Amount = amount; Currency = currency; }
///     protected override IEnumerable&lt;object?&gt; GetEqualityComponents()
///     {
///         yield return Amount;
///         yield return Currency;
///     }
/// }
/// </code>
/// </example>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Returns the set of values that define the identity of this value object.
    /// Order matters: <c>(EUR, 10)</c> ≠ <c>(10, EUR)</c>.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <inheritdoc/>
    public bool Equals(ValueObject? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents(),
                EqualityComparer<object?>.Default);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ValueObject);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var component in GetEqualityComponents())
            hash.Add(component);
        return hash.ToHashCode();
    }

    /// <summary>Two value objects are equal when all components are equal.</summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
        => left?.Equals(right) ?? right is null;

    /// <summary>Two value objects differ when any component differs.</summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !(left == right);
}
