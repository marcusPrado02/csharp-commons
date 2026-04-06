namespace MarcusPrado.Platform.PerformanceTestKit.Scenarios;

/// <summary>
/// A load-test scenario that measures the throughput of an arbitrary async command
/// (e.g., a CQRS command handler invocation) under concurrent load.
/// </summary>
public sealed class CommandThroughputScenario
{
    private readonly Func<CancellationToken, Task> _command;
    private readonly LoadTestConfig _config;

    /// <summary>
    /// Initialises a new <see cref="CommandThroughputScenario"/>.
    /// </summary>
    /// <param name="command">The async command to execute on every virtual-user iteration.</param>
    /// <param name="config">Load test configuration (VUs, duration, optional warmup).</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="command"/> or <paramref name="config"/> is <see langword="null"/>.
    /// </exception>
    public CommandThroughputScenario(Func<CancellationToken, Task> command, LoadTestConfig config)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(config);

        _command = command;
        _config = config;
    }

    /// <summary>
    /// Executes the scenario and returns aggregated load test results.
    /// </summary>
    /// <param name="ct">Optional cancellation token to abort the run early.</param>
    /// <returns>A <see cref="LoadTestResult"/> with throughput and latency statistics.</returns>
    public Task<LoadTestResult> RunAsync(CancellationToken ct = default) =>
        LoadTestRunner.RunAsync(_config, _command, ct);
}
