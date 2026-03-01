namespace MarcusPrado.Platform.Domain.SeedWork;

/// <summary>
/// Thrown when an <see cref="IBusinessRule"/> is broken inside an aggregate.
/// Carry the violated rule so that callers can inspect it for logging or mapping.
/// </summary>
/// <remarks>
/// Usage inside an aggregate root:
/// <code>
/// protected static void CheckRule(IBusinessRule rule)
/// {
///     if (rule.IsBroken())
///         throw new BusinessRuleViolationException(rule);
/// }
/// </code>
/// </remarks>
public sealed class BusinessRuleViolationException : DomainException
{
    /// <summary>The specific rule that was broken.</summary>
    public IBusinessRule BrokenRule { get; }

    /// <summary>Initialises with the violated <paramref name="rule"/>.</summary>
    public BusinessRuleViolationException(IBusinessRule rule)
        : base(rule.Message)
    {
        ArgumentNullException.ThrowIfNull(rule, nameof(rule));
        BrokenRule = rule;
    }
}
