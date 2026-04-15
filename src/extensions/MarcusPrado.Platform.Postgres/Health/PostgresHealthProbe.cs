using MarcusPrado.Platform.Postgres.Connection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace MarcusPrado.Platform.Postgres.Health;

/// <summary>
/// <see cref="IHealthCheck"/> that executes <c>SELECT version()</c> against PostgreSQL.
/// </summary>
public sealed class PostgresHealthProbe : IHealthCheck
{
    private readonly PostgresConnectionFactory _factory;

    /// <summary>Initialises the probe with the given connection factory.</summary>
    public PostgresHealthProbe(PostgresConnectionFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory;
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var rows = await _factory.QueryAsync<string>("SELECT version()", ct: cancellationToken);
            var version = rows.FirstOrDefault() ?? "unknown";
            return HealthCheckResult.Healthy($"PostgreSQL {version}");
        }
        catch (NpgsqlException ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL unreachable", ex);
        }
    }
}
