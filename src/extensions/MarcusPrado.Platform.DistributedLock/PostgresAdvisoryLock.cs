using System.Data;

namespace MarcusPrado.Platform.DistributedLock;

/// <summary>
/// PostgreSQL advisory lock implementation using <c>pg_try_advisory_xact_lock</c>.
/// Accepts a <see cref="IDbConnection"/> so that the implementation can be used
/// with any ADO.NET-compatible PostgreSQL driver (e.g. Npgsql) or mocked in tests.
/// </summary>
/// <remarks>
/// Because <c>pg_try_advisory_xact_lock</c> is transaction-scoped, the caller is
/// responsible for managing the transaction lifecycle on the provided connection.
/// The returned <see cref="IAsyncDisposable"/> is a no-op because the lock is released
/// automatically when the transaction commits or rolls back.
/// </remarks>
public sealed class PostgresAdvisoryLock : IDistributedLock
{
    private readonly IDbConnection _connection;

    /// <summary>
    /// Initialises a new instance using the provided <see cref="IDbConnection"/>.
    /// </summary>
    /// <param name="connection">
    /// An open (or openable) database connection to the PostgreSQL instance.
    /// </param>
    public PostgresAdvisoryLock(IDbConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        _connection = connection;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The <paramref name="expiry"/> parameter is ignored because PostgreSQL advisory
    /// transaction locks are automatically released at transaction end; there is no
    /// TTL mechanism.
    /// </remarks>
    public Task<IAsyncDisposable?> AcquireAsync(
        string key,
        TimeSpan expiry,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        ct.ThrowIfCancellationRequested();

        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        // Derive a stable int64 key from the string key using a simple hash.
        var lockId = (long)key.GetHashCode();

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT pg_try_advisory_xact_lock(@lockId)";

        var param = cmd.CreateParameter();
        param.ParameterName = "lockId";
        param.Value = lockId;
        cmd.Parameters.Add(param);

        var result = cmd.ExecuteScalar();
        var acquired = result is true || result?.ToString()?.Equals("t", StringComparison.OrdinalIgnoreCase) == true
                       || result?.ToString()?.Equals("True", StringComparison.OrdinalIgnoreCase) == true;

        IAsyncDisposable? handle = acquired ? new NoOpDisposable() : null;
        return Task.FromResult(handle);
    }

    /// <summary>
    /// A no-op <see cref="IAsyncDisposable"/> returned when a PostgreSQL advisory lock
    /// is successfully acquired. The lock is released by the database when the transaction
    /// ends; no explicit release action is needed.
    /// </summary>
    private sealed class NoOpDisposable : IAsyncDisposable
    {
        /// <inheritdoc/>
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
