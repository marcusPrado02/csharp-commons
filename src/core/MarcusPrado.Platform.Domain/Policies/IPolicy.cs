namespace MarcusPrado.Platform.Domain.Policies;

/// <summary>
/// Encapsulates a domain policy that decides whether a given <typeparamref name="TInput"/>
/// is permitted.  Unlike <c>ISpecification</c>, a policy carries a human-readable
/// denial reason, making it suitable for authorisation and business-rule enforcement
/// at the application boundary.
/// </summary>
/// <typeparam name="TInput">The context / command / entity being evaluated.</typeparam>
public interface IPolicy<TInput>
{
    /// <summary>
    /// Evaluates the policy against the supplied <paramref name="input"/>.
    /// Returns an allowed or denied <see cref="PolicyResult"/>.
    /// </summary>
    PolicyResult Evaluate(TInput input);
}
