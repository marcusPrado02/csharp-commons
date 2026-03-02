using System.Collections.Generic;
using MarcusPrado.Platform.Resilience.Backoff;

namespace MarcusPrado.Platform.Resilience.Policies;

/// <summary>
/// Retry policy with configurable attempt count and back-off strategy.
/// </summary>
public sealed class RetryPolicy
{
    private readonly RetryOptions _options;

    /// <summary>Initialises with the provided options.</summary>
    public RetryPolicy(RetryOptions options) => _options = options;

    /// <summary>
    /// Executes <paramref name="action"/>, retrying on transient failures.
    /// </summary>
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        var exceptions   = new List<Exception>();
        var previousDelay = TimeSpan.Zero;

        for (var attempt = 0; attempt <= _options.MaxRetries; attempt++)
        {
            try
            {
                return await action(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (
                attempt < _options.MaxRetries
                && (_options.ShouldRetry?.Invoke(ex) ?? true))
            {
                exceptions.Add(ex);
                _options.OnRetry?.Invoke(attempt, ex);

                var delay = _options.BackoffStrategy switch
                {
                    BackoffStrategy.Fixed
                        => _options.BaseDelay,
                    BackoffStrategy.Exponential
                        => ExponentialBackoff.Calculate(attempt, _options.BaseDelay),
                    BackoffStrategy.ExponentialWithJitter
                        => DecorrelatedJitterBackoff.Calculate(
                            previousDelay, _options.BaseDelay, _options.MaxDelay),
                    _ => _options.BaseDelay,
                };
                previousDelay = delay;

                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
#pragma warning disable CA1031  // intentional — last resort catch
            catch (Exception ex)
            {
                exceptions.Add(ex);
                break;
            }
#pragma warning restore CA1031
        }

        throw new AggregateException(
            $"Action failed after {_options.MaxRetries} retries.", exceptions);
    }
}
