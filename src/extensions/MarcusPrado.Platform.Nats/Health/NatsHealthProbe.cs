namespace MarcusPrado.Platform.Nats.Health;

/// <summary>
/// Health check that pings the NATS server and reports the connection state.
/// Returns <see cref="HealthStatus.Healthy"/> when the connection is open,
/// and <see cref="HealthStatus.Unhealthy"/> otherwise.
/// </summary>
public sealed class NatsHealthProbe : IHealthCheck
{
    private readonly INatsConnection _connection;

    /// <summary>
    /// Initialises the health probe with an injected NATS connection.
    /// </summary>
    /// <param name="connection">The NATS connection to probe.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="connection"/> is <see langword="null"/>.
    /// </exception>
    public NatsHealthProbe(INatsConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        _connection = connection;
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_connection.ConnectionState != NatsConnectionState.Open)
            {
                return HealthCheckResult.Unhealthy(
                    $"NATS connection is not open (state: {_connection.ConnectionState}).");
            }

            var rtt = await _connection.PingAsync(cancellationToken).ConfigureAwait(false);
            return HealthCheckResult.Healthy($"NATS ping succeeded (RTT: {rtt.TotalMilliseconds:F1} ms).");
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("NATS health check was cancelled.");
        }
        catch (NatsException ex)
        {
            return HealthCheckResult.Unhealthy("NATS connection is unhealthy.", ex);
        }
    }
}
