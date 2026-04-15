using System.Data;
using Dapper;

namespace MarcusPrado.Platform.Postgres.Repository;

/// <summary>
/// Dapper-backed <see cref="IDapperRepository"/> that works with any
/// <see cref="IDbConnection"/> provider (Npgsql, SqlClient, etc.).
/// </summary>
public sealed class DapperRepository : IDapperRepository
{
    private readonly Func<IDbConnection> _connectionFactory;

    /// <summary>Initializes a new instance of <see cref="DapperRepository"/> with the provided connection factory.</summary>
    /// <param name="connectionFactory">A factory that creates and returns an open-ready <see cref="IDbConnection"/>.</param>
    public DapperRepository(Func<IDbConnection> connectionFactory) => _connectionFactory = connectionFactory;

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, CancellationToken ct = default)
    {
        using var conn = Open();
        return (await conn.QueryAsync<T>(sql, param).ConfigureAwait(false)).AsList();
    }

    /// <inheritdoc />
    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, CancellationToken ct = default)
    {
        using var conn = Open();
        return await conn.QueryFirstOrDefaultAsync<T>(sql, param).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<int> ExecuteAsync(string sql, object? param = null, CancellationToken ct = default)
    {
        using var conn = Open();
        return await conn.ExecuteAsync(sql, param).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null, CancellationToken ct = default)
    {
        using var conn = Open();
        return await conn.ExecuteScalarAsync<T>(sql, param).ConfigureAwait(false);
    }

    private IDbConnection Open()
    {
        var conn = _connectionFactory();
        conn.Open();
        return conn;
    }
}
