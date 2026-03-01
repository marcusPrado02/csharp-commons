using MarcusPrado.Platform.Abstractions.Errors;

namespace MarcusPrado.Platform.Abstractions.Results;

/// <summary>
/// Non-generic discriminated union for operations that succeed or fail
/// without returning a meaningful value.
/// </summary>
/// <remarks>
/// <para>
/// Implemented as a <c>readonly struct</c> so that the success path allocates
/// nothing on the heap.
/// </para>
/// <para>
/// Use <see cref="Success()"/> and <see cref="Failure(Error)"/> to construct
/// instances, or leverage the implicit conversion from <see cref="Error"/>:
/// <code>
/// public Result DeleteOrder(Guid id)
/// {
///     if (!_orders.Remove(id))
///         return Error.NotFound("ORDER.NOT_FOUND", $"...");
///     return Result.Success();
/// }
/// </code>
/// </para>
/// <para>
/// Bridge methods <see cref="Success{T}(T)"/> and <see cref="Failure{T}(Error)"/>
/// create typed <see cref="Result{T}"/> instances, allowing a single static entry
/// point for all result construction.
/// </para>
/// </remarks>
public readonly struct Result : IEquatable<Result>
{
    private readonly Error _error;

    private Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        _error = error;
    }

    // ── State ────────────────────────────────────────────────────────────────

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the <see cref="Error"/> associated with this failure.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when accessed on a successful result. Check <see cref="IsFailure"/>
    /// first, or use <see cref="ResultExtensions.Match{TOut}"/> to handle both branches.
    /// </exception>
    public Error Error => IsFailure
        ? _error
        : throw new InvalidOperationException(
            "Cannot access the Error of a successful Result. " +
            "Check IsFailure before accessing Error, or use Match instead.");

    // ── Factory ──────────────────────────────────────────────────────────────

    /// <summary>Creates a successful result.</summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>Creates a failed result with the given <paramref name="error"/>.</summary>
    /// <param name="error">The error describing the failure.</param>
    public static Result Failure(Error error) => new(false, error);

    // ── Bridge to generic ─────────────────────────────────────────────────────

    /// <summary>
    /// Creates a successful <see cref="Result{T}"/> carrying <paramref name="value"/>.
    /// </summary>
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    /// <summary>
    /// Creates a failed <see cref="Result{T}"/> with the given <paramref name="error"/>.
    /// </summary>
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);

    // ── Implicit conversions ─────────────────────────────────────────────────

    /// <summary>
    /// Implicitly converts an <see cref="Error"/> into a failed <see cref="Result"/>.
    /// Enables <c>return error;</c> in methods returning <see cref="Result"/>.
    /// </summary>
    public static implicit operator Result(Error error) => Failure(error);

    // ── Equality ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public bool Equals(Result other) =>
        IsSuccess == other.IsSuccess && _error.Equals(other._error);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Result other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(IsSuccess, _error);

    /// <inheritdoc/>
    public static bool operator ==(Result left, Result right) => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(Result left, Result right) => !left.Equals(right);

    // ── Formatting ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override string ToString() =>
        IsSuccess
            ? "Result { IsSuccess = true }"
            : $"Result {{ IsSuccess = false, Error = {_error} }}";
}
