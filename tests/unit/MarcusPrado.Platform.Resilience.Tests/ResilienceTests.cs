using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MarcusPrado.Platform.Resilience.Backoff;
using MarcusPrado.Platform.Resilience.Execution;
using MarcusPrado.Platform.Resilience.Overload;
using MarcusPrado.Platform.Resilience.Policies;
using Xunit;

namespace MarcusPrado.Platform.Resilience.Tests;

public sealed class RetryPolicyTests
{
    [Fact]
    public async Task ExecuteAsync_SuccessOnFirstAttempt_ReturnsResult()
    {
        var policy = new RetryPolicy(new RetryOptions { MaxRetries = 3 });

        var result = await policy.ExecuteAsync(_ => Task.FromResult(42));

        result.Should().Be(42);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessAfterTwoFailures_ReturnsResult()
    {
        var attempts = 0;
        var policy   = new RetryPolicy(new RetryOptions
        {
            MaxRetries  = 3,
            BaseDelay   = TimeSpan.FromMilliseconds(1),
        });

        var result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            if (attempts < 3) throw new InvalidOperationException("transient");
            return Task.FromResult(99);
        });

        result.Should().Be(99);
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_ExceedsMaxRetries_ThrowsAggregateException()
    {
        var policy = new RetryPolicy(new RetryOptions
        {
            MaxRetries = 2,
            BaseDelay  = TimeSpan.FromMilliseconds(1),
        });

        var act = async () => await policy.ExecuteAsync<int>(
            _ => throw new InvalidOperationException("fail"));

        await act.Should().ThrowAsync<AggregateException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRetryReturnsFalse_DoesNotRetry()
    {
        var attempts = 0;
        var policy   = new RetryPolicy(new RetryOptions
        {
            MaxRetries  = 5,
            BaseDelay   = TimeSpan.FromMilliseconds(1),
            ShouldRetry = _ => false,
        });

        await Assert.ThrowsAsync<AggregateException>(async () =>
            await policy.ExecuteAsync<int>(_ =>
            {
                attempts++;
                throw new InvalidOperationException("fail");
            }));

        attempts.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_CancellationRequested_DoesNotRetry()
    {
        var policy = new RetryPolicy(new RetryOptions { MaxRetries = 5 });
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var act = async () => await policy.ExecuteAsync<int>(
            ct => Task.FromCanceled<int>(ct), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}

public sealed class CircuitBreakerPolicyTests
{
    [Fact]
    public void InitialState_IsClosed()
    {
        var policy = new CircuitBreakerPolicy(new CircuitBreakerOptions());
        policy.State.Should().Be(CircuitBreakerState.Closed);
    }

    [Fact]
    public async Task ExecuteAsync_AfterFailureThreshold_OpensCircuit()
    {
        var policy = new CircuitBreakerPolicy(new CircuitBreakerOptions
        {
            FailureThreshold = 3,
        });

        for (var i = 0; i < 3; i++)
        {
#pragma warning disable CA1031
            try { await policy.ExecuteAsync<int>(_ => throw new InvalidOperationException()); }
            catch { /* swallow */ }
#pragma warning restore CA1031
        }

        policy.State.Should().Be(CircuitBreakerState.Open);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOpen_ThrowsCircuitBreakerOpenException()
    {
        var policy = new CircuitBreakerPolicy(new CircuitBreakerOptions
        {
            FailureThreshold = 1,
            BreakDuration    = TimeSpan.FromHours(1),
        });

#pragma warning disable CA1031
        try { await policy.ExecuteAsync<int>(_ => throw new InvalidOperationException()); }
        catch { /* trip the breaker */ }
#pragma warning restore CA1031

        var act = async () => await policy.ExecuteAsync(_ => Task.FromResult(1));
        await act.Should().ThrowAsync<CircuitBreakerOpenException>();
    }
}

public sealed class HedgingPolicyTests
{
    [Fact]
    public async Task ExecuteAsync_FastPrimary_ReturnsPrimaryResult()
    {
        var policy = new HedgingPolicy(new HedgingOptions
        {
            HedgingDelay = TimeSpan.FromSeconds(10),
        });

        var result = await policy.ExecuteAsync(_ => Task.FromResult(7));

        result.Should().Be(7);
    }

    [Fact]
    public async Task ExecuteAsync_SlowPrimary_ReturnsHedgedResult()
    {
        var policy = new HedgingPolicy(new HedgingOptions
        {
            HedgingDelay = TimeSpan.FromMilliseconds(10),
        });

        var callCount = 0;
        var result = await policy.ExecuteAsync(async ct =>
        {
            var n = Interlocked.Increment(ref callCount);
            if (n == 1) await Task.Delay(500, ct);   // primary is slow
            return n;
        });

        result.Should().BeGreaterThan(0);
    }
}

public sealed class TimeoutPolicyTests
{
    [Fact]
    public async Task ExecuteAsync_CompletesWithinTimeout_ReturnsResult()
    {
        var policy = new TimeoutPolicy(TimeSpan.FromSeconds(5));

        var result = await policy.ExecuteAsync(_ => Task.FromResult(3));

        result.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_ExceedsTimeout_ThrowsTimeoutException()
    {
        var policy = new TimeoutPolicy(TimeSpan.FromMilliseconds(50));

        var act = async () => await policy.ExecuteAsync(
            async ct => { await Task.Delay(5000, ct); return 0; });

        await act.Should().ThrowAsync<TimeoutException>();
    }
}

public sealed class BulkheadPolicyTests
{
    [Fact]
    public async Task ExecuteAsync_WithinLimit_Succeeds()
    {
        using var policy = new BulkheadPolicy(2);

        var result = await policy.ExecuteAsync(_ => Task.FromResult(5));

        result.Should().Be(5);
    }

    [Fact]
    public async Task ExecuteAsync_ExceedsLimit_ThrowsBulkheadRejectedException()
    {
        using var policy = new BulkheadPolicy(1);
        var tcs = new TaskCompletionSource<int>();

        // occupy the single slot
        var held = policy.ExecuteAsync(_ => tcs.Task);

        // second call should be rejected immediately
        var act = async () => await policy.ExecuteAsync(_ => Task.FromResult(0));
        await act.Should().ThrowAsync<BulkheadRejectedException>();

        tcs.SetResult(1);
        await held;
    }
}

public sealed class AdaptiveConcurrencyLimiterTests
{
    [Fact]
    public async Task ExecuteAsync_WithinLimit_Succeeds()
    {
        var limiter = new AdaptiveConcurrencyLimiter(10);

        var result = await limiter.ExecuteAsync(_ => Task.FromResult(1));

        result.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_LimitOf1_TwoParallelCalls_SecondThrowsOverload()
    {
        var limiter = new AdaptiveConcurrencyLimiter(1);
        var tcs     = new TaskCompletionSource<int>();

        // occupy the single slot
        var first = limiter.ExecuteAsync(_ => tcs.Task);

        await Task.Delay(20); // let first call enter

        var act = async () => await limiter.ExecuteAsync(_ => Task.FromResult(0));
        await act.Should().ThrowAsync<OverloadException>();

        tcs.SetResult(1);
        await first;
    }
}

public sealed class ResilientExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_NoPoliciess_ReturnsResult()
    {
        using var executor = new ResilientExecutor();

        var result = await executor.ExecuteAsync(_ => Task.FromResult(42));

        result.Should().Be(42);
    }

    [Fact]
    public async Task ExecuteAsync_WithRetry_RetriesOnFailure()
    {
        var attempts = 0;
        using var executor = new ResilientExecutor()
            .WithRetry(new RetryOptions
            {
                MaxRetries = 2,
                BaseDelay  = TimeSpan.FromMilliseconds(1),
            });

        var result = await executor.ExecuteAsync(_ =>
        {
            attempts++;
            if (attempts < 2) throw new InvalidOperationException("transient");
            return Task.FromResult(9);
        });

        result.Should().Be(9);
    }

    [Fact]
    public async Task ExecuteAsync_WithTimeout_RespectsTimeout()
    {
        using var executor = new ResilientExecutor()
            .WithTimeout(TimeSpan.FromMilliseconds(50));

        var act = async () => await executor.ExecuteAsync(
            async ct => { await Task.Delay(5000, ct); return 0; });

        await act.Should().ThrowAsync<TimeoutException>();
    }
}

public sealed class BackoffTests
{
    [Fact]
    public void ExponentialBackoff_DoublesEachAttempt()
    {
        var d0 = ExponentialBackoff.Calculate(0, TimeSpan.FromMilliseconds(100));
        var d1 = ExponentialBackoff.Calculate(1, TimeSpan.FromMilliseconds(100));
        var d2 = ExponentialBackoff.Calculate(2, TimeSpan.FromMilliseconds(100));

        d0.TotalMilliseconds.Should().BeApproximately(100,  0.001);
        d1.TotalMilliseconds.Should().BeApproximately(200,  0.001);
        d2.TotalMilliseconds.Should().BeApproximately(400,  0.001);
    }

    [Fact]
    public void DecorrelatedJitter_StaysWithinBounds()
    {
        var baseDelay = TimeSpan.FromMilliseconds(100);
        var maxDelay  = TimeSpan.FromSeconds(2);

        for (var i = 0; i < 50; i++)
        {
            var delay = DecorrelatedJitterBackoff.Calculate(
                TimeSpan.Zero, baseDelay, maxDelay);

            delay.Should().BeGreaterOrEqualTo(baseDelay);
            delay.Should().BeLessOrEqualTo(maxDelay);
        }
    }
}
