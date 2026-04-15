using FluentAssertions;
using MarcusPrado.Platform.PerformanceTestKit;
using MarcusPrado.Platform.PerformanceTestKit.Scenarios;
using Xunit;

namespace MarcusPrado.Platform.PerformanceTestKit.Tests;

public sealed class PerformanceTestKitTests
{
    // ── Scenario 1 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadTestRunner_SingleVu_CompletesWithinDuration()
    {
        // Arrange
        var config = new LoadTestConfig(VirtualUsers: 1, Duration: TimeSpan.FromMilliseconds(200));

        // Act
        var result = await LoadTestRunner.RunAsync(config, _ => Task.CompletedTask);

        // Assert
        result.TotalRequests.Should().BeGreaterThan(0);
        result.ErrorCount.Should().Be(0);
        result.ThroughputRps.Should().BeGreaterThan(0);
    }

    // ── Scenario 2 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadTestRunner_MultipleVus_AllComplete()
    {
        // Arrange
        var config = new LoadTestConfig(VirtualUsers: 3, Duration: TimeSpan.FromMilliseconds(150));

        // Act
        var result = await LoadTestRunner.RunAsync(config, _ => Task.CompletedTask);

        // Assert
        result.TotalRequests.Should().BeGreaterThan(0);
        result.ErrorRate.Should().Be(0);
    }

    // ── Scenario 3 ────────────────────────────────────────────────────────────

    [Fact]
    public void LoadTestResult_P50_IsCalculatedCorrectly()
    {
        // Arrange — 10 known values: 1..10 ms
        double[] latencies = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        // Act
        double p50 = LoadTestRunner.Percentile(latencies, 50);
        double p95 = LoadTestRunner.Percentile(latencies, 95);
        double p99 = LoadTestRunner.Percentile(latencies, 99);

        // Assert
        // P50  → index = ceil(0.50 * 10) - 1 = 5 - 1 = 4  → latencies[4] = 5
        p50.Should().Be(5);
        // P95  → index = ceil(0.95 * 10) - 1 = 10 - 1 = 9 → latencies[9] = 10
        p95.Should().Be(10);
        // P99  → index = ceil(0.99 * 10) - 1 = 10 - 1 = 9 → latencies[9] = 10
        p99.Should().Be(10);
    }

    // ── Scenario 4 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CommandThroughputScenario_RunsSuccessfully()
    {
        // Arrange
        int counter = 0;
        var config = new LoadTestConfig(VirtualUsers: 2, Duration: TimeSpan.FromMilliseconds(200));
        var scenario = new CommandThroughputScenario(
            _ =>
            {
                Interlocked.Increment(ref counter);
                return Task.CompletedTask;
            },
            config
        );

        // Act
        var result = await scenario.RunAsync();

        // Assert
        result.TotalRequests.Should().BeGreaterThan(0);
        result.ErrorCount.Should().Be(0);
        counter.Should().BeGreaterThan(0);
        result.TotalRequests.Should().Be(counter);
    }

    // ── Scenario 5 — MessagingThroughputScenario ─────────────────────────────

    [Fact]
    public async Task MessagingThroughputScenario_RunsSuccessfully()
    {
        // Arrange
        int publishCount = 0;
        var config = new LoadTestConfig(VirtualUsers: 2, Duration: TimeSpan.FromMilliseconds(150));
        var scenario = new MessagingThroughputScenario(
            _ =>
            {
                Interlocked.Increment(ref publishCount);
                return Task.CompletedTask;
            },
            config
        );

        // Act
        var result = await scenario.RunAsync();

        // Assert
        result.TotalRequests.Should().BeGreaterThan(0);
        result.ErrorRate.Should().Be(0);
        publishCount.Should().BeGreaterThan(0);
    }

    // ── Scenario 6 — LoadTestResult.ToReport ─────────────────────────────────

    [Fact]
    public void LoadTestResult_ToReport_ContainsKeyMetrics()
    {
        // Arrange
        var result = new LoadTestResult
        {
            TotalRequests = 1000,
            ErrorCount = 10,
            ThroughputRps = 250.5,
            P50Ms = 12.3,
            P95Ms = 45.6,
            P99Ms = 89.0,
        };

        // Act
        string report = result.ToReport();

        // Assert
        report.Should().Contain("1000");
        report.Should().Contain("250");
        report.Should().Contain("12");
        report.Should().Contain("45");
        report.Should().Contain("89");
    }

    // ── Scenario 7 — error counting ──────────────────────────────────────────

    [Fact]
    public async Task LoadTestRunner_ErrorsAreCounted()
    {
        // Arrange — action always throws
        var config = new LoadTestConfig(VirtualUsers: 1, Duration: TimeSpan.FromMilliseconds(100));

        // Act
        var result = await LoadTestRunner.RunAsync(
            config,
            _ => throw new InvalidOperationException("deliberate failure")
        );

        // Assert
        result.ErrorCount.Should().BeGreaterThan(0);
        result.ErrorRate.Should().Be(1.0); // 100% errors
    }

    // ── Scenario 8 — empty percentile ────────────────────────────────────────

    [Fact]
    public void Percentile_EmptyArray_ReturnsZero()
    {
        double result = LoadTestRunner.Percentile([], 50);
        result.Should().Be(0);
    }
}
