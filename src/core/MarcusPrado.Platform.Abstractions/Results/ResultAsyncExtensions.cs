using MarcusPrado.Platform.Abstractions.Errors;

namespace MarcusPrado.Platform.Abstractions.Results;

/// <summary>
/// Asynchronous extensions for <see cref="Task{TResult}"/> of <see cref="Result{T}"/>,
/// enabling fluent awaitable pipelines without nesting <c>await</c> calls inside
/// <c>if</c> blocks.
/// </summary>
/// <remarks>
/// All overloads use <c>ConfigureAwait(false)</c> so they are safe to call in any
/// synchronization context (library code convention).
/// </remarks>
/// <example>
/// <code>
/// var result = await GetOrderAsync(id)            // Task&lt;Result&lt;Order&gt;&gt;
///     .BindAsync(order => ValidateAsync(order))   // Task&lt;Result&lt;Order&gt;&gt;
///     .MapAsync(order => MapToDto(order))         // Task&lt;Result&lt;OrderDto&gt;&gt;
///     .OnFailureAsync(err => _log.LogWarning("..."));
/// </code>
/// </example>
public static class ResultAsyncExtensions
{
    // ── MapAsync ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Awaits the <paramref name="resultTask"/> and applies the synchronous
    /// <paramref name="mapper"/> to the value if successful.
    /// </summary>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));
        var result = await resultTask.ConfigureAwait(false);
        return result.Map(mapper);
    }

    /// <summary>
    /// Awaits the <paramref name="resultTask"/> and applies the asynchronous
    /// <paramref name="mapper"/> to the value if successful.
    /// </summary>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<TOut>> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure) return result.Error;
        var value = await mapper(result.Value).ConfigureAwait(false);
        return value;
    }

    // ── BindAsync ────────────────────────────────────────────────────────────

    /// <summary>
    /// Awaits the <paramref name="resultTask"/> and chains an asynchronous
    /// fallible operation. Short-circuits on failure.
    /// </summary>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder, nameof(binder));
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure) return result.Error;
        return await binder(result.Value).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the <paramref name="resultTask"/> and chains a synchronous
    /// fallible operation. Short-circuits on failure.
    /// </summary>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Result<TOut>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder, nameof(binder));
        var result = await resultTask.ConfigureAwait(false);
        return result.Bind(binder);
    }

    /// <summary>
    /// Awaits the <paramref name="resultTask"/> and chains a non-generic
    /// asynchronous fallible operation, discarding the value on success.
    /// </summary>
    public static async Task<Result> BindAsync<TIn>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder, nameof(binder));
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure) return result.Error;
        return await binder(result.Value).ConfigureAwait(false);
    }

    // ── MatchAsync ───────────────────────────────────────────────────────────

    /// <summary>
    /// Awaits the <paramref name="resultTask"/> and exhaustively handles both
    /// branches with synchronous handlers.
    /// </summary>
    public static async Task<TOut> MatchAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));
        ArgumentNullException.ThrowIfNull(onFailure, nameof(onFailure));
        var result = await resultTask.ConfigureAwait(false);
        return result.Match(onSuccess, onFailure);
    }

    /// <summary>
    /// Awaits the <paramref name="resultTask"/> and exhaustively handles both
    /// branches with asynchronous handlers.
    /// </summary>
    public static async Task<TOut> MatchAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<TOut>> onSuccess,
        Func<Error, Task<TOut>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));
        ArgumentNullException.ThrowIfNull(onFailure, nameof(onFailure));
        var result = await resultTask.ConfigureAwait(false);
        return result.IsSuccess
            ? await onSuccess(result.Value).ConfigureAwait(false)
            : await onFailure(result.Error).ConfigureAwait(false);
    }

    // ── Tap async ────────────────────────────────────────────────────────────

    /// <summary>
    /// Awaits the <paramref name="resultTask"/> and invokes the asynchronous
    /// <paramref name="action"/> if successful. Returns the original result.
    /// </summary>
    public static async Task<Result<T>> OnSuccessAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action, nameof(action));
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsSuccess) await action(result.Value).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Awaits the <paramref name="resultTask"/> and invokes the asynchronous
    /// <paramref name="action"/> if failed. Returns the original result.
    /// </summary>
    public static async Task<Result<T>> OnFailureAsync<T>(
        this Task<Result<T>> resultTask,
        Func<Error, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action, nameof(action));
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure) await action(result.Error).ConfigureAwait(false);
        return result;
    }

    // ── EnsureAsync ──────────────────────────────────────────────────────────

    /// <summary>
    /// Awaits the <paramref name="resultTask"/> and validates the value with an
    /// asynchronous <paramref name="predicate"/>. Fails with <paramref name="error"/>
    /// when the predicate returns <c>false</c>. Already-failed results pass through.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, Task<bool>> predicate,
        Error error)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        var result = await resultTask.ConfigureAwait(false);
        if (!result.IsSuccess) return result;
        return await predicate(result.Value).ConfigureAwait(false) ? result : error;
    }
}
