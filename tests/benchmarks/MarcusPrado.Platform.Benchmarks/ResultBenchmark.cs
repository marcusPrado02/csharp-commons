using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using MarcusPrado.Platform.Abstractions.Errors;
using MarcusPrado.Platform.Abstractions.Results;

namespace MarcusPrado.Platform.Benchmarks;

/// <summary>
/// Compares the allocation cost of <see cref="Result{T}"/> vs exception-based
/// error propagation across the happy path and the failure path.
///
/// Expected outcome:
///   Success path : Result&lt;T&gt; ≈ 0 bytes (readonly struct, no heap allocation)
///   Failure path : Result&lt;T&gt; ≈ 32 bytes (Error record on heap)
///   Exception    : ~200+ bytes (Exception object + stack trace capture)
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ResultBenchmark
{
    private static readonly Error NotFoundError =
        Error.NotFound("ORDER.NOT_FOUND", "Order was not found.");

    // ── Happy path ──────────────────────────────────────────────────────────

    /// <summary>
    /// Allocates nothing: factory returns a stack-allocated readonly struct.
    /// </summary>
    [Benchmark(Baseline = true, Description = "Result<T> success (no alloc)")]
    public Result<int> ResultSuccess() => Result<int>.Success(42);

    /// <summary>
    /// Implicit conversion from T to Result&lt;T&gt;.
    /// Same cost as explicit Success() factory.
    /// </summary>
    [Benchmark(Description = "Result<T> implicit T→Result")]
    public Result<int> ResultImplicitConversion()
    {
        Result<int> result = 42;
        return result;
    }

    // ── Failure path ─────────────────────────────────────────────────────────

    /// <summary>
    /// Failure wraps the Error record — one small heap allocation.
    /// </summary>
    [Benchmark(Description = "Result<T> failure (Error alloc)")]
    public Result<int> ResultFailure() => Result<int>.Failure(NotFoundError);

    /// <summary>
    /// Deconstruct pattern: typical consumer code unpacks the result.
    /// </summary>
    [Benchmark(Description = "Result<T> Deconstruct")]
    public (bool ok, int val, Error err) ResultDeconstruct()
    {
        var result = Result<int>.Success(42);
        result.Deconstruct(out var ok, out var val, out var error);
        return (ok, val, error);
    }

    // ── Exception baseline ───────────────────────────────────────────────────

    /// <summary>
    /// Throw + catch an exception.
    /// Captures a stack trace — significant allocation + CPU cost.
    /// </summary>
    [Benchmark(Description = "Exception throw+catch")]
    public int ExceptionThrowCatch()
    {
        try
        {
            ThrowNotFound();
            return 0;
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
    }

    private static void ThrowNotFound() =>
        throw new InvalidOperationException("Order was not found.");

    /// <summary>
    /// Pre-allocated exception (no stack trace capture).
    /// Models the theoretical "optimised" exception approach.
    /// </summary>
    [Benchmark(Description = "Exception pre-allocated (no throw)")]
    public int ExceptionPreAllocated()
    {
        var ex = new InvalidOperationException("Order was not found.");
        return ex.Message.Length > 0 ? -1 : 0;
    }

    // ── Chaining ────────────────────────────────────────────────────────────

    /// <summary>
    /// Map: transforms the value without leaving the Result monad.
    /// </summary>
    [Benchmark(Description = "Result<T> Map chain ×3")]
    public Result<string> ResultMapChain() =>
        Result<int>.Success(42)
            .Map(x => x * 2)
            .Map(x => x + 1)
            .Map(x => x.ToString());

    /// <summary>
    /// Bind: sequential operations where each can fail.
    /// </summary>
    [Benchmark(Description = "Result<T> Bind chain ×3")]
    public Result<string> ResultBindChain() =>
        Result<int>.Success(42)
            .Bind(x => Result<int>.Success(x * 2))
            .Bind(x => Result<int>.Success(x + 1))
            .Bind(x => Result<string>.Success(x.ToString()));
}
