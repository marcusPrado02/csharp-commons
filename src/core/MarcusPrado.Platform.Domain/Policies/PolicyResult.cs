namespace MarcusPrado.Platform.Domain.Policies;

/// <summary>
/// Immutable outcome of an <see cref="IPolicy{TInput}"/> evaluation.
/// Use the static factories <see cref="Allow"/> and <see cref="Deny"/> to create instances.
/// </summary>
public sealed record PolicyResult
{
    /// <summary><c>true</c> when the evaluated subject is permitted.</summary>
    public bool IsAllowed { get; private init; }

    /// <summary><c>true</c> when the evaluated subject is denied.</summary>
    public bool IsDenied => !IsAllowed;

    /// <summary>
    /// Human-readable reason for the decision.
    /// Always set on denial; may be empty on allow.
    /// </summary>
    public string Reason { get; private init; } = string.Empty;

    private PolicyResult() { }

    // ── Factories ─────────────────────────────────────────────────────────

    /// <summary>Creates an allowed result with an optional informational reason.</summary>
    public static PolicyResult Allow(string reason = "") => new() { IsAllowed = true, Reason = reason };

    /// <summary>Creates a denied result. A non-empty <paramref name="reason"/> is strongly encouraged.</summary>
    public static PolicyResult Deny(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason, nameof(reason));
        return new() { IsAllowed = false, Reason = reason };
    }

    /// <inheritdoc/>
    public override string ToString() => IsAllowed ? $"Allowed: {Reason}" : $"Denied: {Reason}";
}
