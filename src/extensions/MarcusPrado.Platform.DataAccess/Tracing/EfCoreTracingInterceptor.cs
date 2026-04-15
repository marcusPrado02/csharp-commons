using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MarcusPrado.Platform.DataAccess.Tracing;

/// <summary>
/// EF Core interceptor that creates OpenTelemetry spans for each executed SQL command,
/// with the SQL text sanitized to remove literal values.
/// </summary>
public sealed class EfCoreTracingInterceptor : DbCommandInterceptor
{
    // OTel DB semantic convention attribute names
    private const string DbSystem = "db.system";
    private const string DbStatement = "db.statement";
    private const string DbOperation = "db.operation";

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result
    )
    {
        RecordActivity(command, eventData.Duration);
        return result;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default
    )
    {
        RecordActivity(command, eventData.Duration);
        return new ValueTask<DbDataReader>(result);
    }

    public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
    {
        RecordActivity(command, eventData.Duration);
        return result;
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default
    )
    {
        RecordActivity(command, eventData.Duration);
        return new ValueTask<int>(result);
    }

    private static void RecordActivity(DbCommand command, TimeSpan duration)
    {
        using var activity = DbActivitySource.Instance.StartActivity("db.query", ActivityKind.Client);

        if (activity is null)
            return;

        activity.SetTag(DbSystem, "sql");
        activity.SetTag(DbStatement, SqlSanitizer.Sanitize(command.CommandText));
        activity.SetTag(DbOperation, ExtractOperation(command.CommandText));
        activity.SetTag("db.duration_ms", duration.TotalMilliseconds);
    }

    public static string ExtractOperation(string sql)
    {
        var trimmed = sql.TrimStart();
        var firstWord =
            trimmed.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "UNKNOWN";
        return firstWord.ToUpperInvariant();
    }
}
