using MarcusPrado.Platform.Observability.SLO;
using MarcusPrado.Platform.OpenTelemetry.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.OpenTelemetry.Tests;

/// <summary>Unit tests for SLO / Error Budget tracking.</summary>
public sealed class SloTests
{
    private static readonly ServiceLevelObjective ThreeNinesSlo = new("api-availability", 0.999, TimeSpan.FromDays(30));

    // ── ErrorBudgetCalculator ─────────────────────────────────────────────────

    [Fact]
    public void Calculate_PerfectAvailability_ReturnsFullBudgetRemaining()
    {
        var snapshot = new SloSnapshot(TotalRequests: 1000, FailedRequests: 0);

        var result = ErrorBudgetCalculator.Calculate(snapshot, ThreeNinesSlo);

        result.AvailabilityRate.Should().BeApproximately(1.0, precision: 0.0001);
        result.ErrorBudgetConsumed.Should().BeApproximately(0.0, precision: 0.0001);
        result.IsBudgetExhausted.Should().BeFalse();
    }

    [Fact]
    public void Calculate_ExactlyAtTarget_IsNotExhausted()
    {
        // 999 successes / 1000 = 99.9% availability
        var snapshot = new SloSnapshot(TotalRequests: 1000, FailedRequests: 1);

        var result = ErrorBudgetCalculator.Calculate(snapshot, ThreeNinesSlo);

        result.AvailabilityRate.Should().BeApproximately(0.999, precision: 0.0001);
        result.IsBudgetExhausted.Should().BeFalse();
    }

    [Fact]
    public void Calculate_BelowTarget_IsBudgetExhausted()
    {
        // 990 successes / 1000 = 99.0% < 99.9% target
        var snapshot = new SloSnapshot(TotalRequests: 1000, FailedRequests: 10);

        var result = ErrorBudgetCalculator.Calculate(snapshot, ThreeNinesSlo);

        result.AvailabilityRate.Should().BeApproximately(0.99, precision: 0.0001);
        result.IsBudgetExhausted.Should().BeTrue();
        result.ErrorBudgetConsumed.Should().BeGreaterThan(1.0);
    }

    [Fact]
    public void Calculate_ZeroRequests_ReturnsFullAvailabilityAndBudget()
    {
        var snapshot = new SloSnapshot(TotalRequests: 0, FailedRequests: 0);

        var result = ErrorBudgetCalculator.Calculate(snapshot, ThreeNinesSlo);

        result.AvailabilityRate.Should().BeApproximately(1.0, precision: 0.0001);
        result.IsBudgetExhausted.Should().BeFalse();
        result.ErrorBudgetConsumed.Should().BeApproximately(0.0, precision: 0.0001);
    }

    [Fact]
    public void Calculate_HalfBudgetConsumed_ReturnsCorrectMetrics()
    {
        // Target 99.9%, error budget = 0.1%. Half consumed = 0.05% errors = 0.5 errors per 1000
        // Use 1000 requests, 0 failures → check with partial failure e.g. 500k total, 250 failed
        // error rate = 250/500000 = 0.0005 = 0.05% → half of 0.1% budget
        var snapshot = new SloSnapshot(TotalRequests: 500_000, FailedRequests: 250);

        var result = ErrorBudgetCalculator.Calculate(snapshot, ThreeNinesSlo);

        result.ErrorBudgetConsumed.Should().BeApproximately(0.5, precision: 0.01);
        result.IsBudgetExhausted.Should().BeFalse();
    }

    [Fact]
    public void Calculate_AllRequestsFailed_ExhausesBudgetCompletely()
    {
        var snapshot = new SloSnapshot(TotalRequests: 100, FailedRequests: 100);

        var result = ErrorBudgetCalculator.Calculate(snapshot, ThreeNinesSlo);

        result.AvailabilityRate.Should().BeApproximately(0.0, precision: 0.0001);
        result.IsBudgetExhausted.Should().BeTrue();
        result.ErrorBudgetConsumed.Should().BeGreaterThan(1.0);
        result.ErrorBudgetRemaining.Should().BeLessThan(0.0);
    }

    // ── SloMetricsCollector ───────────────────────────────────────────────────

    [Fact]
    public void SloMetricsCollector_Dispose_DoesNotThrow()
    {
        var snapshot = new SloSnapshot(TotalRequests: 1000, FailedRequests: 1);
        using var collector = new SloMetricsCollector(ThreeNinesSlo, () => snapshot);

        var act = () => collector.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public void SloMetricsCollector_Constructor_NullSlo_Throws()
    {
        var act = () => new SloMetricsCollector(null!, () => new SloSnapshot(100, 0));
        act.Should().Throw<ArgumentNullException>().WithParameterName("slo");
    }

    [Fact]
    public void SloMetricsCollector_Constructor_NullProvider_Throws()
    {
        var act = () => new SloMetricsCollector(ThreeNinesSlo, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("snapshotProvider");
    }

    // ── SloExtensions ─────────────────────────────────────────────────────────

    [Fact]
    public void AddPlatformSlo_RegistersSloMetricsCollector()
    {
        var snapshot = new SloSnapshot(TotalRequests: 1000, FailedRequests: 1);
        var sp = new ServiceCollection().AddPlatformSlo(ThreeNinesSlo, () => snapshot).BuildServiceProvider();

        using var collector = sp.GetService<SloMetricsCollector>();

        collector.Should().NotBeNull();
    }

    // ── SloSnapshot / ServiceLevelObjective records ───────────────────────────

    [Fact]
    public void SloSnapshot_EqualityByValue()
    {
        var a = new SloSnapshot(100, 5);
        var b = new SloSnapshot(100, 5);

        a.Should().Be(b);
    }

    [Fact]
    public void ServiceLevelObjective_EqualityByValue()
    {
        var a = new ServiceLevelObjective("test", 0.99, TimeSpan.FromDays(7));
        var b = new ServiceLevelObjective("test", 0.99, TimeSpan.FromDays(7));

        a.Should().Be(b);
    }
}
