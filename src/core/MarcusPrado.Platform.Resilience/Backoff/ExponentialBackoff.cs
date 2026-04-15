namespace MarcusPrado.Platform.Resilience.Backoff;

/// <summary>Calculates exponential back-off delays for retry policies.</summary>
public static class ExponentialBackoff
{
    /// <summary>
    /// Returns the delay for <paramref name="attempt"/> (0-based).
    /// <c>base * 2^attempt</c>, capped at <paramref name="maxDelay"/>.
    /// </summary>
    public static TimeSpan Calculate(int attempt, TimeSpan baseDelay, TimeSpan? maxDelay = null)
    {
        var raw = baseDelay.TotalMilliseconds * Math.Pow(2, attempt);
        var capped = maxDelay.HasValue ? Math.Min(raw, maxDelay.Value.TotalMilliseconds) : raw;
        return TimeSpan.FromMilliseconds(capped);
    }
}
