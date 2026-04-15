using Microsoft.Extensions.Logging;

namespace MarcusPrado.Platform.Application.Pipeline;

/// <summary>
/// Logs the start, duration, and outcome of every command / query.
/// Registered as the outermost behavior (order 1) so it captures total elapsed time.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    /// <summary>Initializes the behavior with a typed logger.</summary>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default
    )
    {
        var requestName = typeof(TRequest).Name;

#pragma warning disable CA1873
        _logger.LogInformation("Handling {RequestName}", requestName);
#pragma warning restore CA1873

        var started = System.Diagnostics.Stopwatch.GetTimestamp();

        bool succeeded = false;

        try
        {
            var response = await next(cancellationToken);
            succeeded = true;
            return response;
        }
        finally
        {
            var elapsed = System.Diagnostics.Stopwatch.GetElapsedTime(started);
            var elapsedMs = (long)elapsed.TotalMilliseconds;

            if (succeeded)
            {
#pragma warning disable CA1873
                _logger.LogInformation("Handled {RequestName} in {ElapsedMs}ms", requestName, elapsedMs);
#pragma warning restore CA1873
            }
            else
            {
#pragma warning disable CA1873
                _logger.LogWarning("Failed {RequestName} after {ElapsedMs}ms", requestName, elapsedMs);
#pragma warning restore CA1873
            }
        }
    }
}
