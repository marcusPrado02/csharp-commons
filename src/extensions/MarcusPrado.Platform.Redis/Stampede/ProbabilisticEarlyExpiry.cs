namespace MarcusPrado.Platform.Redis.Stampede;

/// <summary>
/// Implements the XFetch algorithm for probabilistic early cache expiration to avoid stampedes.
/// The algorithm computes a score based on the time remaining until expiry, the expected
/// recomputation time (delta), and a tuning factor (beta).
/// A random component ensures that different callers will begin refreshing the cache at
/// slightly different times before expiry, so only one typically wins the race.
/// </summary>
public static class ProbabilisticEarlyExpiry
{
    /// <summary>
    /// Determines whether the cache entry should be refreshed early using the XFetch algorithm.
    /// </summary>
    /// <param name="timeToLive">Remaining time until the cache entry expires.</param>
    /// <param name="delta">Expected time to recompute / fetch the cached value.</param>
    /// <param name="beta">
    /// Tuning parameter controlling aggressiveness of early refresh.
    /// Defaults to <c>1.0</c>; higher values cause earlier refreshes.
    /// Must be greater than zero.
    /// </param>
    /// <returns>
    /// <c>true</c> when the cache entry should be refreshed proactively before it expires,
    /// computed as: refresh when <c>delta * beta * -log(rand) > timeToLive</c>.
    /// </returns>
    /// <remarks>
    /// XFetch formula: refresh when <c>now &gt; expiry - delta * beta * log(rand)</c>
    /// where <c>rand</c> is a uniform random value in (0, 1].
    /// Because <c>log(rand)</c> is negative, the term <c>-delta * beta * log(rand)</c>
    /// is positive — this effectively extends the recompute window before expiry.
    /// </remarks>
    public static bool ShouldRefresh(TimeSpan timeToLive, TimeSpan delta, double beta = 1.0)
    {
        if (beta <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(beta), beta, "beta must be greater than zero.");
        }

        if (timeToLive <= TimeSpan.Zero)
        {
            // Already expired — always refresh
            return true;
        }

        // XFetch: refresh when currentTime > expiry - delta * beta * log(rand)
        // Equivalently: refresh when delta * beta * -log(rand) > timeToLive
        var rand = Random.Shared.NextDouble();
        // Guard against rand being too close to 0 (log(0) is undefined / -infinity)
        if (rand < double.Epsilon)
        {
            rand = double.Epsilon;
        }

        var earlyWindow = delta.TotalSeconds * beta * -Math.Log(rand);
        return earlyWindow > timeToLive.TotalSeconds;
    }
}
