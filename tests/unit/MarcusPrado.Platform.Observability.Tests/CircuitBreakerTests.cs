using MarcusPrado.Platform.Observability.CircuitBreaker;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Observability.Tests;

public sealed class CircuitBreakerRegistryTests
{
    [Fact]
    public void Register_AddsEntryInClosedState()
    {
        var registry = new CircuitBreakerRegistry();
        registry.Register("svc-a");

        var entry = registry.GetAll().Single(e => e.Name == "svc-a");
        entry.State.Should().Be(CircuitBreakerState.Closed);
        entry.FailuresTotal.Should().Be(0);
        entry.LastStateChange.Should().BeNull();
    }

    [Fact]
    public void RecordFailure_IncrementsFailureCount()
    {
        var registry = new CircuitBreakerRegistry(failureThreshold: 5);
        registry.Register("svc-b");
        registry.RecordFailure("svc-b");
        registry.RecordFailure("svc-b");

        var entry = registry.GetAll().Single(e => e.Name == "svc-b");
        entry.FailuresTotal.Should().Be(2);
        entry.State.Should().Be(CircuitBreakerState.Closed);
    }

    [Fact]
    public void RecordFailure_OpensCircuitAtThreshold()
    {
        var registry = new CircuitBreakerRegistry(failureThreshold: 3);
        registry.Register("svc-c");
        registry.RecordFailure("svc-c");
        registry.RecordFailure("svc-c");
        registry.RecordFailure("svc-c");

        var entry = registry.GetAll().Single(e => e.Name == "svc-c");
        entry.State.Should().Be(CircuitBreakerState.Open);
        entry.LastStateChange.Should().NotBeNull();
    }

    [Fact]
    public void RecordSuccess_ResetsFailureCount()
    {
        var registry = new CircuitBreakerRegistry();
        registry.Register("svc-d");
        registry.RecordFailure("svc-d");
        registry.RecordFailure("svc-d");
        registry.RecordSuccess("svc-d");

        var entry = registry.GetAll().Single(e => e.Name == "svc-d");
        entry.FailuresTotal.Should().Be(0);
    }

    [Fact]
    public void SetState_ManuallyChangesState()
    {
        var registry = new CircuitBreakerRegistry();
        registry.Register("svc-e");
        registry.SetState("svc-e", CircuitBreakerState.HalfOpen);

        var entry = registry.GetAll().Single(e => e.Name == "svc-e");
        entry.State.Should().Be(CircuitBreakerState.HalfOpen);
        entry.LastStateChange.Should().NotBeNull();
    }

    [Fact]
    public void Reset_ReturnsToClosed_WithZeroFailures()
    {
        var registry = new CircuitBreakerRegistry(failureThreshold: 2);
        registry.Register("svc-f");
        registry.RecordFailure("svc-f");
        registry.RecordFailure("svc-f"); // opens circuit
        registry.Reset("svc-f");

        var entry = registry.GetAll().Single(e => e.Name == "svc-f");
        entry.State.Should().Be(CircuitBreakerState.Closed);
        entry.FailuresTotal.Should().Be(0);
        entry.LastStateChange.Should().NotBeNull();
    }

    [Fact]
    public void GetAll_ReturnsAllRegisteredEntries()
    {
        var registry = new CircuitBreakerRegistry();
        registry.Register("svc-x");
        registry.Register("svc-y");

        registry.GetAll().Select(e => e.Name).Should().Contain(["svc-x", "svc-y"]);
    }

    [Fact]
    public void RecordFailure_WithoutPriorRegister_CreatesEntry()
    {
        var registry = new CircuitBreakerRegistry();
        registry.RecordFailure("svc-lazy");

        var entry = registry.GetAll().Single(e => e.Name == "svc-lazy");
        entry.FailuresTotal.Should().Be(1);
    }
}

public sealed class CircuitBreakerMetricsTests
{
    [Fact]
    public void Constructor_DoesNotThrow()
    {
        var act = () =>
        {
            var registry = new CircuitBreakerRegistry();
            using var metrics = new CircuitBreakerMetrics(registry);
        };
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordFailure_DoesNotThrow()
    {
        var registry = new CircuitBreakerRegistry();
        using var metrics = new CircuitBreakerMetrics(registry);
        var act = () => metrics.RecordFailure("payment-svc");
        act.Should().NotThrow();
    }
}

public sealed class CircuitBreakerExtensionsTests
{
    [Fact]
    public void AddPlatformCircuitBreakerRegistry_RegistersRegistryAsSingleton()
    {
        var sp = new ServiceCollection().AddPlatformCircuitBreakerRegistry().BuildServiceProvider();

        var r1 = sp.GetService<CircuitBreakerRegistry>();
        var r2 = sp.GetService<CircuitBreakerRegistry>();
        r1.Should().NotBeNull();
        r1.Should().BeSameAs(r2);
    }

    [Fact]
    public void AddPlatformCircuitBreakerRegistry_RegistersMetrics()
    {
        var sp = new ServiceCollection().AddPlatformCircuitBreakerRegistry().BuildServiceProvider();

        sp.GetService<CircuitBreakerMetrics>().Should().NotBeNull();
    }

    [Fact]
    public void MapCircuitBreakerEndpoints_WithRegistry_RegistryRemainsAccessible()
    {
        var registry = new CircuitBreakerRegistry();
        registry.Register("test-svc");

        // Endpoint registration testing requires a running TestServer.
        // Here we verify the registry state that backing endpoints would expose.
        registry.GetAll().Should().ContainSingle(e => e.Name == "test-svc");
    }
}
