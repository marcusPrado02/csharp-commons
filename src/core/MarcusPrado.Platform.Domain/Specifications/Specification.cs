namespace MarcusPrado.Platform.Domain.Specifications;

/// <summary>
/// Abstract base class for domain specifications.
/// Provides fluent <see cref="And"/>, <see cref="Or"/>, and <see cref="Not"/>
/// composition operators so complex rules can be built from simple, testable
/// building blocks without duplicating predicate logic.
/// </summary>
/// <typeparam name="T">The type of domain object this specification evaluates.</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    /// <inheritdoc/>
    public abstract bool IsSatisfiedBy(T candidate);

    // ── Composition ────────────────────────────────────────────────────────────

    /// <summary>Returns a specification that is satisfied when BOTH this and <paramref name="other"/> are.</summary>
    public Specification<T> And(ISpecification<T> other)
    {
        ArgumentNullException.ThrowIfNull(other, nameof(other));
        return new AndSpecification<T>(this, other);
    }

    /// <summary>Returns a specification that is satisfied when EITHER this OR <paramref name="other"/> is.</summary>
    public Specification<T> Or(ISpecification<T> other)
    {
        ArgumentNullException.ThrowIfNull(other, nameof(other));
        return new OrSpecification<T>(this, other);
    }

    /// <summary>Returns a specification that is satisfied when this specification is NOT satisfied.</summary>
    public Specification<T> Not() => new NotSpecification<T>(this);

    // ── Convenience factory ──────────────────────────────────────────────────────

    /// <summary>
    /// Creates a one-off specification from a lambda, useful for queries that
    /// do not warrant a dedicated class.
    /// </summary>
    public static Specification<T> Create(Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        return new PredicateSpecification<T>(predicate);
    }
}

// ── Internal composite implementations ────────────────────────────────────────

file sealed class AndSpecification<T>(ISpecification<T> Left, ISpecification<T> Right) : Specification<T>
{
    public override bool IsSatisfiedBy(T candidate)
        => Left.IsSatisfiedBy(candidate) && Right.IsSatisfiedBy(candidate);
}

file sealed class OrSpecification<T>(ISpecification<T> Left, ISpecification<T> Right) : Specification<T>
{
    public override bool IsSatisfiedBy(T candidate)
        => Left.IsSatisfiedBy(candidate) || Right.IsSatisfiedBy(candidate);
}

file sealed class NotSpecification<T>(ISpecification<T> Inner) : Specification<T>
{
    public override bool IsSatisfiedBy(T candidate)
        => !Inner.IsSatisfiedBy(candidate);
}

file sealed class PredicateSpecification<T>(Func<T, bool> Predicate) : Specification<T>
{
    public override bool IsSatisfiedBy(T candidate) => Predicate(candidate);
}
