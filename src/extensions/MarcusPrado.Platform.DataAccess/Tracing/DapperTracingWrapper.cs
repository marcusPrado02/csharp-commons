using Dapper;

namespace MarcusPrado.Platform.DataAccess.Tracing;

/// <summary>
/// Dapper extension methods that create OpenTelemetry spans around query execution.
/// </summary>
public static class DapperTracingWrapper
{
    public static async Task<IEnumerable<T>> QueryWithTraceAsync<T>(
        this IDbConnection connection,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = DbActivitySource.Instance.StartActivity("db.query", ActivityKind.Client);
        activity?.SetTag("db.system", "sql");
        activity?.SetTag("db.statement", SqlSanitizer.Sanitize(sql));
        activity?.SetTag("db.operation", "SELECT");

        return await connection.QueryAsync<T>(
            new CommandDefinition(sql, param, transaction, cancellationToken: cancellationToken));
    }

    public static async Task<int> ExecuteWithTraceAsync(
        this IDbConnection connection,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = DbActivitySource.Instance.StartActivity("db.execute", ActivityKind.Client);
        activity?.SetTag("db.system", "sql");
        activity?.SetTag("db.statement", SqlSanitizer.Sanitize(sql));
        activity?.SetTag("db.operation", SqlSanitizer.Sanitize(sql).Split(' ')[0].ToUpperInvariant());

        return await connection.ExecuteAsync(
            new CommandDefinition(sql, param, transaction, cancellationToken: cancellationToken));
    }
}
