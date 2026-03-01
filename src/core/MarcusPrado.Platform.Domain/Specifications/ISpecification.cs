namespace MarcusPrado.Platform.Domain.Specifications;

/// <summary>
/// Encapsulates a single composable query / selection rule for domain objects.
/// Combine specifications via <see cref="Specification{T}.And"/>,
/// <see cref="Specification{T}.Or"/>, and <see cref="Specification{T}.Not"/>.
/// </summary>
/// <typeparam name="T">The type of domain object being evaluated.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Returns <c>true</c> when <paramref name="candidate"/> satisfies this specification.
    /// </summary>
    bool IsSatisfiedBy(T candidate);
}
