using MarcusPrado.Platform.Abstractions.Errors;

namespace MarcusPrado.Platform.Domain.Errors;

/// <summary>
/// Centralised factory for <see cref="Error"/> instances that originate inside
/// the domain layer.  Each method encodes the error with a structured,
/// machine-readable code in the format <c>AGGREGATE.ERROR_KIND</c> so that
/// upstream layers can pattern-match or look up descriptions without parsing
/// free-form messages.
/// </summary>
public static class DomainError
{
    // ── NotFound ─────────────────────────────────────────────────────────────

    /// <summary>
    /// The aggregate root identified by <paramref name="id"/> does not exist.
    /// </summary>
    public static Error NotFound(string aggregateName, object id)
        => Error.NotFound(
            code: $"{Normalise(aggregateName)}.NOT_FOUND",
            message: $"{aggregateName} with id '{id}' was not found.")
           .WithMetadata("aggregateName", aggregateName)
           .WithMetadata("id", id?.ToString() ?? string.Empty);

    // ── AlreadyExists ────────────────────────────────────────────────────────

    /// <summary>
    /// A <paramref name="aggregateName"/> with the same
    /// <paramref name="field"/> / <paramref name="value"/> already exists.
    /// </summary>
    public static Error AlreadyExists(string aggregateName, string field, object value)
        => Error.Conflict(
            code: $"{Normalise(aggregateName)}.ALREADY_EXISTS",
            message: $"{aggregateName} with {field} '{value}' already exists.")
           .WithMetadata("aggregateName", aggregateName)
           .WithMetadata("field", field)
           .WithMetadata("value", value?.ToString() ?? string.Empty);

    // ── BusinessRuleViolation ────────────────────────────────────────────────

    /// <summary>
    /// A domain invariant or business rule was violated.
    /// </summary>
    public static Error BusinessRuleViolation(
        string aggregateName,
        string rule,
        string message)
        => Error.Validation(
            code: $"{Normalise(aggregateName)}.BUSINESS_RULE_VIOLATION",
            message: message)
           .WithMetadata("aggregateName", aggregateName)
           .WithMetadata("rule", rule);

    // ── ConcurrencyConflict ───────────────────────────────────────────────────

    /// <summary>
    /// An optimistic-concurrency conflict was detected while modifying the
    /// aggregate identified by <paramref name="id"/>.
    /// </summary>
    public static Error ConcurrencyConflict(string aggregateName, object id)
        => Error.Conflict(
            code: $"{Normalise(aggregateName)}.CONCURRENCY_CONFLICT",
            message: $"{aggregateName} '{id}' was modified by another process. Please reload and retry.")
           .WithMetadata("aggregateName", aggregateName)
           .WithMetadata("id", id?.ToString() ?? string.Empty);

    // ── InvalidStateTransition ────────────────────────────────────────────────

    /// <summary>
    /// The transition from <paramref name="currentState"/> to
    /// <paramref name="targetState"/> is not allowed for the aggregate.
    /// </summary>
    public static Error InvalidStateTransition(
        string aggregateName,
        object currentState,
        object targetState)
        => Error.Validation(
            code: $"{Normalise(aggregateName)}.INVALID_STATE_TRANSITION",
            message: $"{aggregateName} cannot transition from '{currentState}' to '{targetState}'.")
           .WithMetadata("aggregateName", aggregateName)
           .WithMetadata("currentState", currentState?.ToString() ?? string.Empty)
           .WithMetadata("targetState", targetState?.ToString() ?? string.Empty);

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>Converts an aggregate name like "OrderItem" to "ORDER_ITEM".</summary>
    private static string Normalise(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "UNKNOWN";

        // Insert '_' before each uppercase letter that follows a lower/digit,
        // then upper-case the whole thing — converts PascalCase to SCREAMING_SNAKE.
        var chars = name.AsSpan();
        var sb = new System.Text.StringBuilder(name.Length + 4);
        for (var i = 0; i < chars.Length; i++)
        {
            var c = chars[i];
            if (i > 0 && char.IsUpper(c) && !char.IsUpper(chars[i - 1]))
                sb.Append('_');
            sb.Append(char.ToUpperInvariant(c));
        }
        return sb.ToString();
    }
}
