using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MarcusPrado.Platform.IntegrationTestEnvironment;

/// <summary>
/// Waits for all provided test containers to reach a healthy (running) state before
/// allowing tests to proceed.
/// </summary>
public sealed class TestEnvironmentHealthCheck
{
    private static readonly TimeSpan _defaultPollInterval = TimeSpan.FromMilliseconds(500);

    private readonly ILogger<TestEnvironmentHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="TestEnvironmentHealthCheck"/> with a
    /// null logger (no logging output).
    /// </summary>
    public TestEnvironmentHealthCheck()
        : this(NullLogger<TestEnvironmentHealthCheck>.Instance)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="TestEnvironmentHealthCheck"/> with
    /// the specified logger.
    /// </summary>
    /// <param name="logger">The logger to write health-check progress to.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> is <see langword="null"/>.
    /// </exception>
    public TestEnvironmentHealthCheck(ILogger<TestEnvironmentHealthCheck> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Polls all <paramref name="containers"/> until every container reports a
    /// <see cref="TestcontainersStates.Running"/> state, or the <paramref name="timeout"/>
    /// elapses.
    /// </summary>
    /// <param name="containers">The list of containers to wait for.</param>
    /// <param name="timeout">The maximum time to wait before throwing.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="Task"/> that completes once all containers are running.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="containers"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="TimeoutException">
    /// Thrown when one or more containers have not become healthy within <paramref name="timeout"/>.
    /// </exception>
    public async Task WaitForHealthyAsync(
        IReadOnlyList<IContainer> containers,
        TimeSpan timeout,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(containers);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        var deadline = DateTimeOffset.UtcNow.Add(timeout);

        while (!cts.Token.IsCancellationRequested)
        {
            var allRunning = containers.All(c => c.State == TestcontainersStates.Running);

            if (allRunning)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("All {Count} container(s) are healthy.", containers.Count);
                }

                return;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Waiting for containers to become healthy. Deadline: {Deadline}.",
                    deadline);
            }

            await Task.Delay(_defaultPollInterval, cts.Token).ConfigureAwait(false);
        }

        var unhealthy = containers
            .Where(c => c.State != TestcontainersStates.Running)
            .Select(c => c.Name)
            .ToList();

        throw new TimeoutException(
            $"The following container(s) did not become healthy within {timeout}: {string.Join(", ", unhealthy)}");
    }
}
