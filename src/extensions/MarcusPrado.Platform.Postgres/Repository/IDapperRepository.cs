namespace MarcusPrado.Platform.Postgres.Repository;

/// <summary>
/// Lightweight Dapper-based repository for raw SQL access.
/// </summary>
public interface IDapperRepository
{
    /// <summary>Executes a query and returns a list of <typeparamref name="T"/>.</summary>
    Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, CancellationToken ct = default);

    /// <summary>Executes a query and returns the first result, or <see langword="default"/>.</summary>
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, CancellationToken ct = default);

    /// <summary>Executes a non-query SQL statement and returns rows affected.</summary>
    Task<int> ExecuteAsync(string sql, object? param = null, CancellationToken ct = default);

    /// <summary>Executes a scalar SQL statement and returns the first value.</summary>
    Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null, CancellationToken ct = default);
}
