namespace MarcusPrado.Platform.PerformanceTestKit;

/// <summary>
/// Configuration for a load test run, specifying concurrency and duration.
/// </summary>
/// <param name="VirtualUsers">Number of concurrent virtual users to simulate.</param>
/// <param name="Duration">How long the load test should run after the warmup period.</param>
/// <param name="WarmupDuration">Optional warmup period before measurements begin. Defaults to <see langword="null"/> (no warmup).</param>
public sealed record LoadTestConfig(int VirtualUsers, TimeSpan Duration, TimeSpan? WarmupDuration = null);
