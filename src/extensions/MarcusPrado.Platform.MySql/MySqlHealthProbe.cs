using MySqlConnector;

namespace MarcusPrado.Platform.MySql;

/// <summary>
/// <see cref="IHealthCheck"/> that verifies MySQL connectivity by opening a connection.
/// </summary>
public sealed class MySqlHealthProbe : IHealthCheck
{
    private readonly IMySqlConnectionFactory _factory;

    /// <summary>Initialises the probe with the given connection factory.</summary>
    public MySqlHealthProbe(IMySqlConnectionFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory;
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var conn = await _factory.CreateOpenConnectionAsync(cancellationToken);
            return HealthCheckResult.Healthy("MySQL connection OK");
        }
        catch (MySqlException ex)
        {
            return HealthCheckResult.Unhealthy("MySQL connection failed", ex);
        }
        catch (InvalidOperationException ex)
        {
            return HealthCheckResult.Unhealthy("MySQL connection failed", ex);
        }
        catch (OperationCanceledException ex)
        {
            return HealthCheckResult.Unhealthy("MySQL health check was cancelled", ex);
        }
    }
}
