using Dapper;
using MarcusPrado.Platform.Postgres.Options;
using Npgsql;

namespace MarcusPrado.Platform.Postgres.Connection;

/// <summary>Creates and pools Npgsql connections using <see cref="NpgsqlDataSource"/>.</summary>
public sealed class PostgresConnectionFactory : IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    /// <summary>Initialises the factory with the given options.</summary>
    public PostgresConnectionFactory(PostgresOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var builder = new NpgsqlDataSourceBuilder(options.ConnectionString);
        _dataSource = builder.Build();
    }

    /// <summary>Opens and returns a new database connection.</summary>
    public async Task<NpgsqlConnection> OpenAsync(CancellationToken ct = default)
    {
        return await _dataSource.OpenConnectionAsync(ct);
    }

    /// <summary>Executes a raw SQL query via Dapper and returns the results.</summary>
    public async Task<IEnumerable<T>> QueryAsync<T>(
        string sql,
        object? param = null,
        CancellationToken ct = default)
    {
        await using var conn = await OpenAsync(ct);
        return await conn.QueryAsync<T>(sql, param, commandTimeout: 30);
    }

    /// <summary>Executes a non-query SQL command and returns affected rows.</summary>
    public async Task<int> ExecuteAsync(
        string sql,
        object? param = null,
        CancellationToken ct = default)
    {
        await using var conn = await OpenAsync(ct);
        return await conn.ExecuteAsync(sql, param, commandTimeout: 30);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync() => await _dataSource.DisposeAsync();
}
