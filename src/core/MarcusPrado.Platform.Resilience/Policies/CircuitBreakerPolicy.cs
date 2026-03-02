using System.Threading;

namespace MarcusPrado.Platform.Resilience.Policies;

/// <summary>
/// Circuit breaker with Closed / Open / Half-Open state machine.
/// Opens after <see cref="CircuitBreakerOptions.FailureThreshold"/> consecutive
/// failures and stays open for <see cref="CircuitBreakerOptions.BreakDuration"/>.
/// </summary>
public sealed class CircuitBreakerPolicy
{
    private readonly CircuitBreakerOptions _options;
    private readonly object _sync = new();
    private CircuitBreakerState _state = CircuitBreakerState.Closed;
    private int  _consecutiveFailures;
    private DateTime _openedAt = DateTime.MinValue;

    /// <summary>Initialises with the provided options.</summary>
    public CircuitBreakerPolicy(CircuitBreakerOptions options) => _options = options;

    /// <summary>Gets the current circuit state.</summary>
    public CircuitBreakerState State => _state;

    /// <summary>Executes <paramref name="action"/> through the circuit breaker.</summary>
    /// <exception cref="CircuitBreakerOpenException">Thrown when the circuit is open.</exception>
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            if (_state == CircuitBreakerState.Open)
            {
                if (DateTime.UtcNow - _openedAt < _options.BreakDuration)
                {
                    throw new CircuitBreakerOpenException(
                        $"Circuit is open. Retry after {_options.BreakDuration}.");
                }

                _state = CircuitBreakerState.HalfOpen;
                _options.OnHalfOpen?.Invoke();
            }
        }

        try
        {
            var result = await action(cancellationToken).ConfigureAwait(false);

            lock (_sync)
            {
                _consecutiveFailures = 0;
                if (_state == CircuitBreakerState.HalfOpen)
                {
                    _state = CircuitBreakerState.Closed;
                    _options.OnClose?.Invoke();
                }
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            lock (_sync)
            {
                _consecutiveFailures++;
                if (_consecutiveFailures >= _options.FailureThreshold
                    && _state != CircuitBreakerState.Open)
                {
                    _state     = CircuitBreakerState.Open;
                    _openedAt  = DateTime.UtcNow;
                    _options.OnOpen?.Invoke();
                }
            }

            throw;
        }
    }
}
