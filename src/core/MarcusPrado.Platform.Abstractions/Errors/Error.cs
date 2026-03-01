using System.Collections.Frozen;

namespace MarcusPrado.Platform.Abstractions.Errors;

/// <summary>
/// An immutable, allocation-efficient representation of a domain or application error.
/// </summary>
/// <remarks>
/// <para>
/// Designed as a <c>readonly record struct</c> so it can be embedded inside
/// <see cref="Results.Result{T}"/> and <see cref="Results.Result"/> with zero heap
/// allocations on the happy (success) path.
/// </para>
/// <para>
/// Use the static factory methods (<see cref="Validation(string,string)"/>,
/// <see cref="NotFound"/>, <see cref="Technical"/>, …) instead of the primary
/// constructor to benefit from semantic naming and correct default severities.
/// </para>
/// <para>
/// Error codes follow the convention <c>"AGGREGATE.REASON"</c> using
/// SCREAMING_SNAKE_CASE, e.g. <c>"PAYMENT.NOT_FOUND"</c>,
/// <c>"AUTH.TOKEN_EXPIRED"</c>, <c>"ORDER.INVALID_STATE_TRANSITION"</c>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Factory usage
/// var error = Error.NotFound("ORDER.NOT_FOUND", $"Order '{id}' was not found.");
///
/// // Implicit conversion inside a Result-returning method
/// public Result&lt;Order&gt; GetOrder(Guid id) =>
///     _orders.TryGetValue(id, out var o) ? o : Error.NotFound("ORDER.NOT_FOUND", $"...");
/// </code>
/// </example>
public readonly record struct Error
{
    // ── Fields ───────────────────────────────────────────────────────────────

    /// <summary>
    /// A stable, machine-readable error code in the form <c>"DOMAIN.REASON"</c>
    /// (SCREAMING_SNAKE_CASE), e.g. <c>"PAYMENT.NOT_FOUND"</c>.
    /// Must never change across versions — consumers may switch on it.
    /// </summary>
    public string Code { get; init; }

    /// <summary>A human-readable, non-null description of the error.</summary>
    public string Message { get; init; }

    /// <summary>Semantic category used for HTTP status-code mapping and retry decisions.</summary>
    public ErrorCategory Category { get; init; }

    /// <summary>Operational severity used for alerting and log-level selection.</summary>
    public ErrorSeverity Severity { get; init; }

    /// <summary>
    /// Optional structured metadata (e.g. field name, attempted value, entity id).
    /// Stored as a <see cref="FrozenDictionary{TKey, TValue}"/> to guarantee immutability
    /// and fast read performance after construction.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }

    // ── Constructor ──────────────────────────────────────────────────────────

    /// <summary>
    /// Initializes an <see cref="Error"/> with validated fields.
    /// Prefer the static factory methods for ergonomics and correct defaults.
    /// </summary>
    /// <param name="code">
    /// Stable machine-readable code (non-null, non-whitespace).
    /// Convention: <c>"AGGREGATE.REASON"</c>.
    /// </param>
    /// <param name="message">Human-readable description (non-null, non-whitespace).</param>
    /// <param name="category">Semantic category; defaults to <see cref="ErrorCategory.Technical"/>.</param>
    /// <param name="severity">Operational severity; defaults to <see cref="ErrorSeverity.Error"/>.</param>
    /// <param name="metadata">Optional immutable structured metadata.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="code"/> or <paramref name="message"/> is null or whitespace.
    /// </exception>
    public Error(
        string code,
        string message,
        ErrorCategory category = ErrorCategory.Technical,
        ErrorSeverity severity = ErrorSeverity.Error,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code, nameof(code));
        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));

        Code = code;
        Message = message;
        Category = category;
        Severity = severity;
        Metadata = metadata ?? FrozenDictionary<string, object>.Empty;
    }

    // ── Internal sentinel ─────────────────────────────────────────────────────

    /// <summary>
    /// Sentinel value used on the success path to avoid nullability.
    /// Never exposed to consumers — accessing <c>result.Error</c> on a
    /// successful result throws <see cref="InvalidOperationException"/>.
    /// </summary>
    // ── Code-based equality (Error identity = Code) ──────────────────────────

    /// <summary>
    /// Two errors with the same <see cref="Code"/> are considered equal.
    /// Message, severity, and metadata are considered presentation details.
    /// </summary>
    public bool Equals(Error other) => string.Equals(Code, other.Code, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override int GetHashCode() => Code is null ? 0 : StringComparer.Ordinal.GetHashCode(Code);

    // ── Internal sentinel ─────────────────────────────────────────────────────

    internal static readonly Error None = new(
        code: "NONE",
        message: "(no error)",
        category: ErrorCategory.Technical,
        severity: ErrorSeverity.Info);

    // ── Factory methods ───────────────────────────────────────────────────────

    /// <summary>
    /// Creates a <see cref="ErrorCategory.Validation"/> error (HTTP 422 / 400).
    /// </summary>
    public static Error Validation(
        string code,
        string message,
        IReadOnlyDictionary<string, object>? metadata = null)
        => new(code, message, ErrorCategory.Validation, ErrorSeverity.Warning, metadata);

    /// <summary>
    /// Creates a field-level <see cref="ErrorCategory.Validation"/> error with
    /// the field name and attempted value stored in <see cref="Metadata"/>.
    /// </summary>
    public static Error Validation(
        string code,
        string message,
        string fieldName,
        object? attemptedValue = null)
        => new(
            code,
            message,
            ErrorCategory.Validation,
            ErrorSeverity.Warning,
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["field"] = fieldName,
                ["attemptedValue"] = attemptedValue!,
            }.ToFrozenDictionary()!);

    /// <summary>
    /// Creates a <see cref="ErrorCategory.NotFound"/> error (HTTP 404).
    /// </summary>
    public static Error NotFound(
        string code,
        string message,
        IReadOnlyDictionary<string, object>? metadata = null)
        => new(code, message, ErrorCategory.NotFound, ErrorSeverity.Info, metadata);

    /// <summary>
    /// Creates a <see cref="ErrorCategory.Conflict"/> error (HTTP 409).
    /// </summary>
    public static Error Conflict(
        string code,
        string message,
        IReadOnlyDictionary<string, object>? metadata = null)
        => new(code, message, ErrorCategory.Conflict, ErrorSeverity.Warning, metadata);

    /// <summary>
    /// Creates an <see cref="ErrorCategory.Unauthorized"/> error (HTTP 401).
    /// </summary>
    public static Error Unauthorized(string code, string message)
        => new(code, message, ErrorCategory.Unauthorized, ErrorSeverity.Warning);

    /// <summary>
    /// Creates a <see cref="ErrorCategory.Forbidden"/> error (HTTP 403).
    /// </summary>
    public static Error Forbidden(string code, string message)
        => new(code, message, ErrorCategory.Forbidden, ErrorSeverity.Warning);

    /// <summary>
    /// Creates a <see cref="ErrorCategory.Technical"/> error (HTTP 500).
    /// Use for unexpected internal failures.
    /// </summary>
    public static Error Technical(
        string code,
        string message,
        IReadOnlyDictionary<string, object>? metadata = null)
        => new(code, message, ErrorCategory.Technical, ErrorSeverity.Error, metadata);

    /// <summary>
    /// Creates an <see cref="ErrorCategory.External"/> error (HTTP 502).
    /// Use when a downstream dependency returns an error.
    /// </summary>
    public static Error External(
        string code,
        string message,
        IReadOnlyDictionary<string, object>? metadata = null)
        => new(code, message, ErrorCategory.External, ErrorSeverity.Error, metadata);

    /// <summary>
    /// Creates a <see cref="ErrorCategory.Timeout"/> error (HTTP 504).
    /// </summary>
    public static Error Timeout(string code, string message)
        => new(code, message, ErrorCategory.Timeout, ErrorSeverity.Error);

    /// <summary>
    /// Creates an <see cref="ErrorCategory.Unavailable"/> error (HTTP 503).
    /// </summary>
    public static Error Unavailable(string code, string message)
        => new(code, message, ErrorCategory.Unavailable, ErrorSeverity.Error);

    // ── Fluent enrichment ────────────────────────────────────────────────────

    /// <summary>
    /// Returns a copy of this error enriched with an additional metadata entry.
    /// Creates a new <see cref="FrozenDictionary{TKey, TValue}"/> on each call;
    /// prefer building full metadata upfront when multiple entries are needed.
    /// </summary>
    public Error WithMetadata(string key, object value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

        var dict = Metadata is not null
            ? new Dictionary<string, object>(Metadata, StringComparer.Ordinal) { [key] = value }
            : new Dictionary<string, object>(StringComparer.Ordinal) { [key] = value };

        return this with { Metadata = dict.ToFrozenDictionary() };
    }

    /// <summary>
    /// Returns a copy of this error with the severity overridden.
    /// Useful when the same error code warrants different severities in different contexts.
    /// </summary>
    public Error WithSeverity(ErrorSeverity severity) => this with { Severity = severity };

    // ── Formatting ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override string ToString() =>
        Metadata is { Count: > 0 }
            ? $"[{Code}] {Message} | metadata: {string.Join(", ", Metadata.Select(kv => $"{kv.Key}={kv.Value}"))}"
            : $"[{Code}] {Message}";
}
