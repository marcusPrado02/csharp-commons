using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MarcusPrado.Platform.HealthChecks.Startup;

/// <summary>
/// An <see cref="IHostedService"/> that runs all registered <see cref="IStartupVerification"/>
/// implementations on application startup. If any verification fails the application is stopped
/// via <see cref="IHostApplicationLifetime.StopApplication"/>.
/// </summary>
public sealed partial class StartupVerificationHostedService : IHostedService
{
    private readonly IEnumerable<IStartupVerification> _verifications;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<StartupVerificationHostedService> _logger;

    /// <summary>Initialises the service with required dependencies.</summary>
    /// <param name="verifications">All registered startup verifications.</param>
    /// <param name="lifetime">Used to stop the application when a verification fails.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public StartupVerificationHostedService(
        IEnumerable<IStartupVerification> verifications,
        IHostApplicationLifetime lifetime,
        ILogger<StartupVerificationHostedService> logger)
    {
        ArgumentNullException.ThrowIfNull(verifications);
        ArgumentNullException.ThrowIfNull(lifetime);
        ArgumentNullException.ThrowIfNull(logger);

        _verifications = verifications;
        _lifetime      = lifetime;
        _logger        = logger;
    }

    /// <summary>
    /// Runs all verifications. Stops the application if any verification fails.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for startup.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var failures = new List<VerificationResult>();

        foreach (var verification in _verifications)
        {
            LogRunning(_logger, verification.Name);
            var result = await verification.VerifyAsync(cancellationToken);

            if (!result.Success)
            {
                LogFailed(_logger, result.Name, result.ErrorMessage ?? string.Empty);
                failures.Add(result);
            }
            else
            {
                LogPassed(_logger, verification.Name);
            }
        }

        if (failures.Count > 0)
        {
            LogStopping(_logger, failures.Count);
            _lifetime.StopApplication();
        }
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    // ── LoggerMessage source generation ──────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Information, Message = "Running startup verification: {Name}")]
    private static partial void LogRunning(ILogger logger, string name);

    [LoggerMessage(Level = LogLevel.Error, Message = "Startup verification failed: {Name} — {Error}")]
    private static partial void LogFailed(ILogger logger, string name, string error);

    [LoggerMessage(Level = LogLevel.Information, Message = "Startup verification passed: {Name}")]
    private static partial void LogPassed(ILogger logger, string name);

    [LoggerMessage(Level = LogLevel.Critical, Message = "{FailureCount} startup verification(s) failed. Stopping application.")]
    private static partial void LogStopping(ILogger logger, int failureCount);
}
