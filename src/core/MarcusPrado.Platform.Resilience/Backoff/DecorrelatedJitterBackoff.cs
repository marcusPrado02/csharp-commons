namespace MarcusPrado.Platform.Resilience.Backoff;

/// <summary>
/// Decorrelated jitter backoff — avoids thundering-herd retry storms.
/// Based on the AWS "Exponential Backoff and Jitter" blog post.
/// </summary>
public static class DecorrelatedJitterBackoff
{
    /// <summary>
    /// Returns a random delay between <paramref name="baseDelay"/> and three
    /// times the previous delay, bounded by <paramref name="maxDelay"/>.
    /// </summary>
    public static TimeSpan Calculate(TimeSpan previousDelay, TimeSpan baseDelay, TimeSpan? maxDelay = null)
    {
        var cap = maxDelay ?? TimeSpan.FromSeconds(30);
        var minMs = baseDelay.TotalMilliseconds;
        var maxMs = Math.Min(
            cap.TotalMilliseconds,
            previousDelay == TimeSpan.Zero ? (baseDelay.TotalMilliseconds * 3) : (previousDelay.TotalMilliseconds * 3)
        );

        return TimeSpan.FromMilliseconds((Random.Shared.NextDouble() * (maxMs - minMs)) + minMs);
    }
}
