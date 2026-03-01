using MarcusPrado.Platform.Abstractions.Errors;

namespace MarcusPrado.Platform.Abstractions.Results;

/// <summary>
/// A discriminated union representing either a successful value of type
/// <typeparamref name="T"/> or an <see cref="Error"/>.
/// </summary>
/// <typeparam name="T">The type of the value carried on the success path.</typeparam>
/// <remarks>
/// <para>
/// Implemented as a <c>readonly struct</c> to avoid heap allocations on the happy
/// path and to make allocations on the error path (boxing of the struct) explicit.
/// </para>
/// <para>
/// Implicit conversion operators allow returning a bare <c>T</c> or <c>Error</c>
/// from methods declared as returning <c>Result&lt;T&gt;</c>, keeping call-site
/// code concise without sacrificing explicitness:
/// <code>
/// public Result&lt;Order&gt; GetOrder(Guid id)
/// {
///     if (!_store.TryGetValue(id, out var order))
///         return Error.NotFound("ORDER.NOT_FOUND", $"Order '{id}' was not found.");
///     return order;  // implicit T → Result&lt;T&gt;
/// }
/// </code>
/// </para>
/// <para>
/// Use <see cref="ResultExtensions.Match{TIn,TOut}"/> for exhaustive pattern matching,
/// and the async counterparts in <see cref="ResultAsyncExtensions"/> for awaitable pipelines.
/// </para>
/// </remarks>
public readonly struct Result<T> : IEquatable<Result<T>>
{
    private readonly T? _value;
    private readonly Error _error;

    private Result(T value)
    {
        IsSuccess = true;
        _value = value;
        _error = Error.None;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        _value = default;
        _error = error;
    }

    // ── State ────────────────────────────────────────────────────────────────

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the successful value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when accessed on a failed result.
    /// Check <see cref="IsSuccess"/> before accessing <see cref="Value"/>,
    /// or use <see cref="ResultExtensions.Match{TIn,TOut}"/> to handle both branches.
    /// </exception>
    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException(
                $"Cannot access Value of a failed Result<{typeof(T).Name}>. "
                    + $"Error: {_error}. "
                    + "Check IsSuccess before accessing Value, or use Match/Map instead."
            );

    /// <summary>
    /// Gets the <see cref="Error"/> associated with this failure.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when accessed on a successful result.
    /// Check <see cref="IsFailure"/> first, or use <see cref="ResultExtensions.Match{TIn,TOut}"/>.
    /// </exception>
    public Error Error =>
        IsFailure
            ? _error
            : throw new InvalidOperationException(
                $"Cannot access Error of a successful Result<{typeof(T).Name}>. "
                    + "Check IsFailure before accessing Error, or use Match instead."
            );

    // ── Factory ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a successful result wrapping <paramref name="value"/>.
    /// </summary>
    public static Result<T> Success(T value) => new(value);

    /// <summary>
    /// Creates a failed result with the given <paramref name="error"/>.
    /// </summary>
    public static Result<T> Failure(Error error) => new(error);

    // ── Implicit conversions ─────────────────────────────────────────────────

    /// <summary>
    /// Implicitly wraps <paramref name="value"/> in a successful result.
    /// Enables <c>return value;</c> in methods returning <c>Result&lt;T&gt;</c>.
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicitly wraps <paramref name="error"/> in a failed result.
    /// Enables <c>return error;</c> in methods returning <c>Result&lt;T&gt;</c>.
    /// </summary>
    public static implicit operator Result<T>(Error error) => Failure(error);

    /// <summary>
    /// Widens a typed result to a non-generic <see cref="Result"/>, discarding the value.
    /// Useful when a caller only cares about success/failure, not the value.
    /// </summary>
    public static implicit operator Result(Result<T> result) =>
        result.IsSuccess ? Result.Success() : Result.Failure(result._error);

    // ── Deconstruct ──────────────────────────────────────────────────────────

    /// <summary>
    /// Deconstructs the result for use in switch expressions and positional patterns.
    /// </summary>
    /// <example>
    /// <code>
    /// var (ok, value, error) = GetOrder(id);
    /// if (ok) Console.WriteLine(value!.Name);
    /// </code>
    /// </example>
    public void Deconstruct(out bool isSuccess, out T? value, out Error error)
    {
        isSuccess = IsSuccess;
        value = _value;
        error = _error;
    }

    // ── Equality ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public bool Equals(Result<T> other)
    {
        if (IsSuccess != other.IsSuccess)
            return false;
        return IsSuccess ? EqualityComparer<T>.Default.Equals(_value, other._value) : _error.Equals(other._error);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Result<T> other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => IsSuccess ? HashCode.Combine(true, _value) : HashCode.Combine(false, _error);

    /// <inheritdoc/>
    public static bool operator ==(Result<T> left, Result<T> right) => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(Result<T> left, Result<T> right) => !left.Equals(right);

    // ── Formatting ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override string ToString() =>
        IsSuccess
            ? $"Result<{typeof(T).Name}> {{ IsSuccess = true, Value = {_value} }}"
            : $"Result<{typeof(T).Name}> {{ IsSuccess = false, Error = {_error} }}";
}
