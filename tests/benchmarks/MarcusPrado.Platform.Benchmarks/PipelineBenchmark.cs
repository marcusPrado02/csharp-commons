using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace MarcusPrado.Platform.Benchmarks;

/// <summary>
/// Measures the overhead of the CQRS pipeline delegate chain as the
/// number of registered behaviors scales.
///
/// The pipeline is a linked list of <see cref="RequestHandlerDelegate{T}"/>
/// closures, mirroring the implementation in
/// <c>MarcusPrado.Platform.Application</c>.
///
/// Expected outcome: overhead is O(N) in number of behaviors, but each
/// step is a simple virtual call — should be &lt;1 µs per added behavior.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class PipelineBenchmark
{
    // ── Minimal pipeline plumbing (mirrors Application abstractions) ─────────

    private delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

    private interface IPipelineBehavior<TRequest, TResponse>
    {
        Task<TResponse> HandleAsync(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken ct);
    }

    private sealed class NoOpBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly string _name;

        public NoOpBehavior(string name) => _name = name;

        public Task<TResponse> HandleAsync(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken ct) =>
            next(); // pure pass-through, no alloc beyond the delegate call
    }

    // ── Pipeline builder ─────────────────────────────────────────────────────

    private sealed record SampleCommand(int Value);

    private static Task<int> BuildAndRunPipeline(
        SampleCommand command,
        int behaviorCount,
        CancellationToken ct = default)
    {
        // Terminal handler
        RequestHandlerDelegate<int> handler = () => Task.FromResult(command.Value * 2);

        // Wrap in N behaviors (reverse order so first behavior executes first)
        for (var i = behaviorCount - 1; i >= 0; i--)
        {
            var behavior = new NoOpBehavior<SampleCommand, int>($"Behavior{i}");
            var next = handler; // capture current tail
            handler = () => behavior.HandleAsync(command, next, ct);
        }

        return handler();
    }

    private static readonly SampleCommand Command = new(21);

    // ── Benchmarks ───────────────────────────────────────────────────────────

    [Benchmark(Baseline = true, Description = "Pipeline — 0 behaviors (direct handler)")]
    public Task<int> Pipeline_0Behaviors() => BuildAndRunPipeline(Command, 0);

    [Benchmark(Description = "Pipeline — 2 behaviors")]
    public Task<int> Pipeline_2Behaviors() => BuildAndRunPipeline(Command, 2);

    [Benchmark(Description = "Pipeline — 4 behaviors")]
    public Task<int> Pipeline_4Behaviors() => BuildAndRunPipeline(Command, 4);

    [Benchmark(Description = "Pipeline — 8 behaviors (full platform stack)")]
    public Task<int> Pipeline_8Behaviors() => BuildAndRunPipeline(Command, 8);

    // ── Synchronous equivalent (no async overhead) ───────────────────────────

    [Benchmark(Description = "Pipeline — 8 behaviors (sync delegate chain)")]
    public int Pipeline_8Behaviors_Sync()
    {
        Func<int> handler = () => Command.Value * 2;
        for (var i = 7; i >= 0; i--)
        {
            var next = handler;
            handler = () => next();
        }
        return handler();
    }
}
