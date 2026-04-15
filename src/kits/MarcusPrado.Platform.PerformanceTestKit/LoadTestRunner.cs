using System.Collections.Concurrent;
using System.Diagnostics;

namespace MarcusPrado.Platform.PerformanceTestKit;

/// <summary>
/// Core load test runner that spawns virtual users and collects latency samples.
/// </summary>
public sealed class LoadTestRunner
{
    /// <summary>
    /// Runs the given <paramref name="action"/> concurrently across the number of virtual users
    /// specified in <paramref name="config"/> for the configured duration, then aggregates results.
    /// </summary>
    /// <param name="config">Load test configuration (VUs, duration, optional warmup).</param>
    /// <param name="action">The async action executed by each virtual user on every iteration.</param>
    /// <param name="ct">Optional cancellation token to abort the run early.</param>
    /// <returns>
    /// A <see cref="LoadTestResult"/> containing throughput and P50/P95/P99 latency statistics.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="config"/> or <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    public static async Task<LoadTestResult> RunAsync(
        LoadTestConfig config,
        Func<CancellationToken, Task> action,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(action);

        // Warmup phase — run without collecting measurements
        if (config.WarmupDuration.HasValue && config.WarmupDuration.Value > TimeSpan.Zero)
        {
            using var warmupCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            warmupCts.CancelAfter(config.WarmupDuration.Value);
            await RunAllVusAsync(config.VirtualUsers, action, new RunContext(), collectSamples: false, warmupCts.Token)
                .ConfigureAwait(false);
        }

        // Measurement phase
        using var measureCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        measureCts.CancelAfter(config.Duration);

        var context = new RunContext();
        var measureSw = Stopwatch.StartNew();
        await RunAllVusAsync(config.VirtualUsers, action, context, collectSamples: true, measureCts.Token)
            .ConfigureAwait(false);
        measureSw.Stop();

        var latencies = context.Latencies.ToArray();
        Array.Sort(latencies);

        long errorCount = context.ErrorCount;
        long totalRequests = latencies.Length + errorCount;
        double elapsedSeconds = measureSw.Elapsed.TotalSeconds;
        double throughput = elapsedSeconds > 0 ? totalRequests / elapsedSeconds : 0;

        return new LoadTestResult
        {
            TotalRequests = totalRequests,
            ErrorCount = errorCount,
            ThroughputRps = throughput,
            P50Ms = Percentile(latencies, 50),
            P95Ms = Percentile(latencies, 95),
            P99Ms = Percentile(latencies, 99),
        };
    }

    // CA1068: CancellationToken is the last parameter
    private static Task RunAllVusAsync(
        int virtualUsers,
        Func<CancellationToken, Task> action,
        RunContext context,
        bool collectSamples,
        CancellationToken ct
    )
    {
        var tasks = new Task[virtualUsers];
        for (int i = 0; i < virtualUsers; i++)
        {
            tasks[i] = RunSingleVuAsync(action, context, collectSamples, ct);
        }

        return Task.WhenAll(tasks);
    }

    // CA1068: CancellationToken is the last parameter
    private static async Task RunSingleVuAsync(
        Func<CancellationToken, Task> action,
        RunContext context,
        bool collectSamples,
        CancellationToken ct
    )
    {
        var sw = new Stopwatch();
        while (!ct.IsCancellationRequested)
        {
            sw.Restart();
            try
            {
                await action(ct).ConfigureAwait(false);
                sw.Stop();
                if (collectSamples)
                {
                    context.Latencies.Add(sw.Elapsed.TotalMilliseconds);
                }
            }
            catch (OperationCanceledException oce) when (oce.CancellationToken == ct || ct.IsCancellationRequested)
            {
                // Normal shutdown triggered by the run duration expiring
                break;
            }
            catch (OperationCanceledException)
            {
                // Cancellation from within the action (e.g. per-request timeout) — count as error
                Interlocked.Increment(ref context.ErrorCount);
            }
            catch (HttpRequestException)
            {
                Interlocked.Increment(ref context.ErrorCount);
            }
            catch (InvalidOperationException)
            {
                Interlocked.Increment(ref context.ErrorCount);
            }
            catch (IOException)
            {
                Interlocked.Increment(ref context.ErrorCount);
            }
            catch (TimeoutException)
            {
                Interlocked.Increment(ref context.ErrorCount);
            }
        }
    }

    /// <summary>
    /// Calculates the given percentile value from a pre-sorted array of latency samples.
    /// </summary>
    /// <param name="sorted">A sorted array of latency values in milliseconds.</param>
    /// <param name="percentile">Percentile to calculate (e.g., 50, 95, 99).</param>
    /// <returns>The latency value at the requested percentile, or <c>0</c> if the array is empty.</returns>
    public static double Percentile(double[] sorted, int percentile)
    {
        ArgumentNullException.ThrowIfNull(sorted);

        if (sorted.Length == 0)
        {
            return 0;
        }

        int index = (int)Math.Ceiling(percentile / 100.0 * sorted.Length) - 1;
        index = Math.Max(0, Math.Min(index, sorted.Length - 1));
        return sorted[index];
    }

    /// <summary>
    /// Internal mutable state shared across VU tasks during a single run phase.
    /// </summary>
    private sealed class RunContext
    {
        /// <summary>Thread-safe bag of successful request latencies in milliseconds.</summary>
        public ConcurrentBag<double> Latencies { get; } = new();

        /// <summary>Count of requests that raised an unexpected exception.</summary>
        public long ErrorCount;
    }
}
