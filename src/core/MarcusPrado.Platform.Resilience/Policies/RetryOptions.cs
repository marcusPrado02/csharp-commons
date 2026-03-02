namespace MarcusPrado.Platform.Resilience.Policies;

/// <summary>Configuration for <see cref="RetryPolicy"/>.</summary>
public sealed class RetryOptions
{
    /// <summary>Maximum number of retry attempts (default 3).</summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>Base delay between attempts (default 200 ms).</summary>
    public TimeSpan BaseDelay { get; init; } = TimeSpan.FromMilliseconds(200);

    /// <summary>Maximum delay cap for jitter strategies (default 30 s).</summary>
    public TimeSpan? MaxDelay { get; init; }

    /// <summary>Back-off strategy (default exponential with jitter).</summary>
    public BackoffStrategy BackoffStrategy { get; init; } = BackoffStrategy.ExponentialWithJitter;

    /// <summary>
    /// Predicate that decides whether an exception is retryable.
    /// When <see langword="null"/>, all exceptions are retried.
    /// </summary>
    public Func<Exception, bool>? ShouldRetry { get; init; }

    /// <summary>Optional callback invoked before each retry sleep.</summary>
    public Action<int, Exception>? OnRetry { get; init; }
}

/// <summary>Back-off delay calculation strategies.</summary>
public enum BackoffStrategy
{
    /// <summary>Constant delay between every attempt.</summary>
    Fixed,

    /// <summary>Exponentially increasing delay.</summary>
    Exponential,

    /// <summary>Exponential growth with decorrelated jitter to avoid thundering herds.</summary>
    ExponentialWithJitter,
}
