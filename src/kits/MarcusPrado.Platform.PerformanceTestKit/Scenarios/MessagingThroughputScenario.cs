namespace MarcusPrado.Platform.PerformanceTestKit.Scenarios;

/// <summary>
/// A placeholder load-test scenario for measuring message-broker throughput
/// (e.g., Kafka or RabbitMQ publish operations).
/// </summary>
/// <remarks>
/// Connect your broker-specific publish logic via the <c>publishAction</c> constructor
/// parameter. The scenario delegates entirely to <see cref="LoadTestRunner"/> so all
/// standard metrics (P50/P95/P99, throughput, error rate) are reported automatically.
/// </remarks>
public sealed class MessagingThroughputScenario
{
    private readonly Func<CancellationToken, Task> _publishAction;
    private readonly LoadTestConfig _config;

    /// <summary>
    /// Initialises a new <see cref="MessagingThroughputScenario"/>.
    /// </summary>
    /// <param name="publishAction">
    /// An async delegate that publishes one message to the target broker per invocation.
    /// </param>
    /// <param name="config">Load test configuration (VUs, duration, optional warmup).</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="publishAction"/> or <paramref name="config"/> is <see langword="null"/>.
    /// </exception>
    public MessagingThroughputScenario(Func<CancellationToken, Task> publishAction, LoadTestConfig config)
    {
        ArgumentNullException.ThrowIfNull(publishAction);
        ArgumentNullException.ThrowIfNull(config);

        _publishAction = publishAction;
        _config = config;
    }

    /// <summary>
    /// Executes the messaging throughput scenario and returns aggregated results.
    /// </summary>
    /// <param name="ct">Optional cancellation token to abort the run early.</param>
    /// <returns>A <see cref="LoadTestResult"/> with throughput and latency statistics.</returns>
    public Task<LoadTestResult> RunAsync(CancellationToken ct = default) =>
        LoadTestRunner.RunAsync(_config, _publishAction, ct);
}
