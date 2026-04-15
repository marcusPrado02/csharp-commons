using Microsoft.Extensions.Logging;

namespace MarcusPrado.Platform.Application.Pipeline;

/// <summary>
/// Retries transient failures a configurable number of times with exponential back-off.
/// Registered as the eighth behavior (order 8), closest to the handler.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int MaxRetries = 3;
    private static readonly TimeSpan _baseDelay = TimeSpan.FromMilliseconds(100);

    private readonly ILogger<RetryBehavior<TRequest, TResponse>> _logger;

    /// <summary>Initializes the behavior with a typed logger.</summary>
    public RetryBehavior(ILogger<RetryBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        var attempt = 0;

        while (true)
        {
            try
            {
                return await next(cancellationToken);
            }
            catch (Exception ex) when (IsTransient(ex) && attempt < MaxRetries)
            {
                attempt++;

                var delay = TimeSpan.FromMilliseconds(
                    _baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));

                _logger.LogWarning(
                    ex,
                    "Transient failure on {RequestType} (attempt {Attempt}/{Max}). Retrying in {DelayMs}ms",
                    typeof(TRequest).Name,
                    attempt,
                    MaxRetries,
                    (long)delay.TotalMilliseconds);

                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    private static bool IsTransient(Exception ex) =>
        ex is TimeoutException
        or OperationCanceledException
        or System.IO.IOException;
}
