namespace MyDomain.Domain.ValueObjects;

/// <summary>Base value object with structural equality.</summary>
public abstract class ValueObject
{
    /// <summary>Returns the equality components.</summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;
        return ((ValueObject)obj).GetEqualityComponents().SequenceEqual(GetEqualityComponents());
    }

    /// <inheritdoc/>
    public override int GetHashCode() =>
        GetEqualityComponents().Aggregate(0, (hash, item) => HashCode.Combine(hash, item));
}
