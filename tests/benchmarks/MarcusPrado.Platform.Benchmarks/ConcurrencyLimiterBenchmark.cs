using System.Threading.RateLimiting;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace MarcusPrado.Platform.Benchmarks;

/// <summary>
/// Benchmarks rate-limiter / concurrency-limiter primitives that underpin
/// <c>MarcusPrado.Platform.Resilience.AdaptiveConcurrencyLimiter</c>.
///
/// Limiters compared:
///   - <see cref="SemaphoreSlim"/>                   — classic mutex approach
///   - <see cref="ConcurrencyLimiter"/>              — .NET 7+ System.Threading.RateLimiting
///   - <see cref="SlidingWindowRateLimiter"/>        — time-based sliding window
///   - <see cref="TokenBucketRateLimiter"/>          — burst-allowing token bucket
///
/// All benchmarks measure the acquire → work → release round-trip to reflect
/// real-world middleware usage (not just the acquire cost alone).
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ConcurrencyLimiterBenchmark : IDisposable
{
    // ── Configured instances (created once, reused across iterations) ─────────

    private SemaphoreSlim _semaphore = default!;
    private ConcurrencyLimiter _concurrencyLimiter = default!;
    private SlidingWindowRateLimiter _slidingWindow = default!;
    private TokenBucketRateLimiter _tokenBucket = default!;

    [GlobalSetup]
    public void Setup()
    {
        _semaphore = new SemaphoreSlim(10, 10);

        _concurrencyLimiter = new ConcurrencyLimiter(
            new ConcurrencyLimiterOptions
            {
                PermitLimit = 10,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            }
        );

        _slidingWindow = new SlidingWindowRateLimiter(
            new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromSeconds(1),
                SegmentsPerWindow = 4,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            }
        );

        _tokenBucket = new TokenBucketRateLimiter(
            new TokenBucketRateLimiterOptions
            {
                TokenLimit = 100,
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                TokensPerPeriod = 100,
                AutoReplenishment = true,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            }
        );
    }

    [GlobalCleanup]
    public void Cleanup() => Dispose();

    // ── Benchmarks ───────────────────────────────────────────────────────────

    [Benchmark(Baseline = true, Description = "SemaphoreSlim (classic mutex)")]
    public async Task SemaphoreSlim_AcquireRelease()
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            await SimulateWorkAsync().ConfigureAwait(false);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    [Benchmark(Description = "ConcurrencyLimiter (System.Threading)")]
    public async Task ConcurrencyLimiter_AcquireRelease()
    {
        using var lease = await _concurrencyLimiter.AcquireAsync(permitCount: 1).ConfigureAwait(false);
        if (lease.IsAcquired)
        {
            await SimulateWorkAsync().ConfigureAwait(false);
        }
    }

    [Benchmark(Description = "SlidingWindowRateLimiter")]
    public async Task SlidingWindow_AcquireRelease()
    {
        using var lease = await _slidingWindow.AcquireAsync(permitCount: 1).ConfigureAwait(false);
        if (lease.IsAcquired)
        {
            await SimulateWorkAsync().ConfigureAwait(false);
        }
    }

    [Benchmark(Description = "TokenBucketRateLimiter")]
    public async Task TokenBucket_AcquireRelease()
    {
        using var lease = await _tokenBucket.AcquireAsync(permitCount: 1).ConfigureAwait(false);
        if (lease.IsAcquired)
        {
            await SimulateWorkAsync().ConfigureAwait(false);
        }
    }

    // ── Contended (8 concurrent acquires) ────────────────────────────────────

    [Benchmark(Description = "SemaphoreSlim contended ×8")]
    public Task SemaphoreSlim_Contended()
    {
        return Task.WhenAll(
            Enumerable
                .Range(0, 8)
                .Select(async _ =>
                {
                    await _semaphore.WaitAsync().ConfigureAwait(false);
                    try
                    {
                        await SimulateWorkAsync().ConfigureAwait(false);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                })
        );
    }

    [Benchmark(Description = "ConcurrencyLimiter contended ×8")]
    public Task ConcurrencyLimiter_Contended()
    {
        return Task.WhenAll(
            Enumerable
                .Range(0, 8)
                .Select(async _ =>
                {
                    using var lease = await _concurrencyLimiter.AcquireAsync(1).ConfigureAwait(false);
                    if (lease.IsAcquired)
                    {
                        await SimulateWorkAsync().ConfigureAwait(false);
                    }
                })
        );
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Task SimulateWorkAsync() => Task.CompletedTask; // no I/O noise

    public void Dispose()
    {
        _semaphore?.Dispose();
        _concurrencyLimiter?.Dispose();
        _slidingWindow?.Dispose();
        _tokenBucket?.Dispose();
    }
}
