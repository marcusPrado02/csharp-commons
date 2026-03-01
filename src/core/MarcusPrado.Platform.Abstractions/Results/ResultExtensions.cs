using System.Collections.Frozen;
using MarcusPrado.Platform.Abstractions.Errors;

namespace MarcusPrado.Platform.Abstractions.Results;

/// <summary>
/// Synchronous functional extensions for <see cref="Result{T}"/> and <see cref="Result"/>.
/// </summary>
/// <remarks>
/// All methods are pure (no side effects, except the intentional side effects of
/// <see cref="OnSuccess{T}"/> and <see cref="OnFailure{T}"/>) and null-check their
/// delegate parameters eagerly.
/// </remarks>
public static class ResultExtensions
{
    // ── Map ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Projects the success value through <paramref name="mapper"/> producing a
    /// <see cref="Result{TOut}"/>.  If the input result is a failure the error is
    /// forwarded unchanged and <paramref name="mapper"/> is never invoked.
    /// </summary>
    /// <typeparam name="TIn">The source value type.</typeparam>
    /// <typeparam name="TOut">The projected value type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="mapper">Pure transformation applied to the success value.</param>
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));
        return result.IsSuccess ? mapper(result.Value) : result.Error;
    }

    /// <summary>
    /// Projects a non-generic <see cref="Result"/> to a <see cref="Result{TOut}"/>
    /// using <paramref name="mapper"/> on the success path.
    /// </summary>
    public static Result<TOut> Map<TOut>(
        this Result result,
        Func<TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));
        return result.IsSuccess ? mapper() : result.Error;
    }

    // ── Bind ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Chains a subsequent fallible operation (<paramref name="binder"/>).
    /// If the input is already a failure, <paramref name="binder"/> is not invoked
    /// and the existing error is forwarded.
    /// </summary>
    /// <typeparam name="TIn">The source value type.</typeparam>
    /// <typeparam name="TOut">The next operation's value type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="binder">The next operation, which itself may succeed or fail.</param>
    public static Result<TOut> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder, nameof(binder));
        return result.IsSuccess ? binder(result.Value) : result.Error;
    }

    /// <summary>
    /// Chains a subsequent fallible operation that discards the input value.
    /// </summary>
    public static Result<TOut> Bind<TOut>(
        this Result result,
        Func<Result<TOut>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder, nameof(binder));
        return result.IsSuccess ? binder() : result.Error;
    }

    /// <summary>
    /// Chains a subsequent non-generic fallible operation on a typed result,
    /// discarding the value on success.
    /// </summary>
    public static Result Bind<TIn>(
        this Result<TIn> result,
        Func<TIn, Result> binder)
    {
        ArgumentNullException.ThrowIfNull(binder, nameof(binder));
        return result.IsSuccess ? binder(result.Value) : result.Error;
    }

    // ── Match ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Exhaustively handles both branches of a <see cref="Result{T}"/>:
    /// applies <paramref name="onSuccess"/> on the happy path and
    /// <paramref name="onFailure"/> on the error path.
    /// </summary>
    /// <typeparam name="TIn">The source value type.</typeparam>
    /// <typeparam name="TOut">The return type produced by both handlers.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">Handler invoked with the value when successful.</param>
    /// <param name="onFailure">Handler invoked with the error when failed.</param>
    /// <returns>The output of whichever handler was invoked.</returns>
    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));
        ArgumentNullException.ThrowIfNull(onFailure, nameof(onFailure));
        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);
    }

    /// <summary>
    /// Non-generic overload of <see cref="Match{TIn,TOut}"/>.
    /// </summary>
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));
        ArgumentNullException.ThrowIfNull(onFailure, nameof(onFailure));
        return result.IsSuccess ? onSuccess() : onFailure(result.Error);
    }

    // ── Tap (OnSuccess / OnFailure) ───────────────────────────────────────────

    /// <summary>
    /// Executes a side-effecting <paramref name="action"/> when the result is
    /// successful, then returns the original result unchanged (tap / passthrough).
    /// </summary>
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action, nameof(action));
        if (result.IsSuccess) action(result.Value);
        return result;
    }

    /// <summary>
    /// Executes a side-effecting <paramref name="action"/> when the result has
    /// failed, then returns the original result unchanged (tap / passthrough).
    /// </summary>
    public static Result<T> OnFailure<T>(this Result<T> result, Action<Error> action)
    {
        ArgumentNullException.ThrowIfNull(action, nameof(action));
        if (result.IsFailure) action(result.Error);
        return result;
    }

    /// <inheritdoc cref="OnSuccess{T}(Result{T},Action{T})"/>
    public static Result OnSuccess(this Result result, Action action)
    {
        ArgumentNullException.ThrowIfNull(action, nameof(action));
        if (result.IsSuccess) action();
        return result;
    }

    /// <inheritdoc cref="OnFailure{T}(Result{T},Action{Error})"/>
    public static Result OnFailure(this Result result, Action<Error> action)
    {
        ArgumentNullException.ThrowIfNull(action, nameof(action));
        if (result.IsFailure) action(result.Error);
        return result;
    }

    // ── Ensure ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Validates the value of an already-successful result using
    /// <paramref name="predicate"/>. If the predicate returns <c>false</c>,
    /// the result is turned into a failure with <paramref name="error"/>.
    /// If the result is already a failure it is returned unchanged.
    /// </summary>
    public static Result<T> Ensure<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Error error)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        if (!result.IsSuccess) return result;
        return predicate(result.Value) ? result : error;
    }

    // ── MapError ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Transforms the error of a failed result through <paramref name="mapper"/>.
    /// If the result is successful it is returned unchanged.
    /// </summary>
    public static Result<T> MapError<T>(
        this Result<T> result,
        Func<Error, Error> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));
        return result.IsFailure ? mapper(result.Error) : result;
    }

    // ── Conversion helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Converts a typed <see cref="Result{T}"/> to a non-generic <see cref="Result"/>,
    /// discarding the value on the success path.
    /// </summary>
    public static Result ToResult<T>(this Result<T> result) =>
        result.IsSuccess ? Result.Success() : Result.Failure(result.Error);

    /// <summary>
    /// Returns the value if the result is successful; otherwise returns
    /// <paramref name="defaultValue"/> (which defaults to <c>default(T)</c>).
    /// </summary>
    public static T? GetValueOrDefault<T>(this Result<T> result, T? defaultValue = default) =>
        result.IsSuccess ? result.Value : defaultValue;

    /// <summary>
    /// Returns the value if the result is successful; otherwise invokes
    /// <paramref name="fallback"/> with the error to produce an alternative value.
    /// </summary>
    public static T GetValueOrElse<T>(this Result<T> result, Func<Error, T> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback, nameof(fallback));
        return result.IsSuccess ? result.Value : fallback(result.Error);
    }

    // ── Combine ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Combines a sequence of results into a single result containing a
    /// read-only list of all values, returning the first failure encountered
    /// and short-circuiting the remainder.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="results">The sequence of results to combine.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> wrapping all values if every element
    /// succeeded, or the first failure otherwise.
    /// </returns>
    public static Result<IReadOnlyList<T>> Combine<T>(this IEnumerable<Result<T>> results)
    {
        ArgumentNullException.ThrowIfNull(results, nameof(results));

        var list = new List<T>();
        foreach (var result in results)
        {
            if (result.IsFailure) return result.Error;
            list.Add(result.Value);
        }

        return Result<IReadOnlyList<T>>.Success(list.AsReadOnly());
    }

    /// <summary>
    /// Combines all results, collecting ALL failures rather than short-circuiting.
    /// Returns <c>Success</c> with all values only when every result succeeds.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="results">The results to validate.</param>
    /// <param name="aggregateCode">Error code used for the aggregate failure.</param>
    /// <param name="aggregateMessage">Message used for the aggregate failure.</param>
    public static Result<IReadOnlyList<T>> CombineAll<T>(
        this IEnumerable<Result<T>> results,
        string aggregateCode = "VALIDATION.MULTIPLE_ERRORS",
        string aggregateMessage = "One or more validation errors occurred.")
    {
        ArgumentNullException.ThrowIfNull(results, nameof(results));

        var successes = new List<T>();
        var failures = new List<(int Index, Error Error)>();

        foreach (var (idx, result) in results.Select((r, i) => (i, r)))
        {
            if (result.IsSuccess) successes.Add(result.Value);
            else failures.Add((idx, result.Error));
        }

        if (failures.Count == 0) return Result<IReadOnlyList<T>>.Success(successes.AsReadOnly());

        var metadataDict = new Dictionary<string, object>(failures.Count * 2);
        for (var i = 0; i < failures.Count; i++)
        {
            metadataDict[$"errors[{i}].code"]    = failures[i].Error.Code;
            metadataDict[$"errors[{i}].message"] = failures[i].Error.Message;
        }

        return Error.Validation(aggregateCode, aggregateMessage, metadataDict.ToFrozenDictionary());
    }
}
