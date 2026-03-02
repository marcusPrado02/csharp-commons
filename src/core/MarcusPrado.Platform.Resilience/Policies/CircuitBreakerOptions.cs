namespace MarcusPrado.Platform.Resilience.Policies;

/// <summary>Configuration for <see cref="CircuitBreakerPolicy"/>.</summary>
public sealed class CircuitBreakerOptions
{
    /// <summary>Number of consecutive failures before the circuit opens (default 5).</summary>
    public int FailureThreshold { get; init; } = 5;

    /// <summary>Duration the circuit stays open before sampling again (default 30 s).</summary>
    public TimeSpan BreakDuration { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>Callback invoked when the circuit opens.</summary>
    public Action? OnOpen { get; init; }

    /// <summary>Callback invoked when the circuit closes after recovery.</summary>
    public Action? OnClose { get; init; }

    /// <summary>Callback invoked when the circuit transitions to half-open.</summary>
    public Action? OnHalfOpen { get; init; }
}

/// <summary>Possible states of a <see cref="CircuitBreakerPolicy"/>.</summary>
public enum CircuitBreakerState
{
    /// <summary>Normal operation; requests pass through.</summary>
    Closed,

    /// <summary>Circuit is open; requests are rejected immediately.</summary>
    Open,

    /// <summary>One trial request is permitted to test recovery.</summary>
    HalfOpen,
}

/// <summary>Thrown when an action is attempted while the circuit is open.</summary>
public sealed class CircuitBreakerOpenException : Exception
{
    /// <inheritdoc />
    public CircuitBreakerOpenException(string message) : base(message) { }
}
