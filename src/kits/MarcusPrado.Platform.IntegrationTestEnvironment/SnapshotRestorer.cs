using System.Text;

namespace MarcusPrado.Platform.IntegrationTestEnvironment;

/// <summary>
/// Tracks a set of database table names and generates SQL scripts for restoring
/// a database to a clean state by truncating all registered tables.
/// </summary>
/// <remarks>
/// This type generates SQL only — it does not execute queries directly.
/// Callers are responsible for obtaining a database connection and executing
/// the script returned by <see cref="GetTruncateScript"/>.
/// </remarks>
public sealed class SnapshotRestorer
{
    private readonly List<string> _tables = [];

    /// <summary>
    /// Registers a table name to be included in the truncate script.
    /// </summary>
    /// <param name="tableName">
    /// The name of the table to register. Can be schema-qualified (e.g. <c>"public.orders"</c>).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="tableName"/> is <see langword="null"/>.
    /// </exception>
    public void RegisterTable(string tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        _tables.Add(tableName);
    }

    /// <summary>
    /// Returns a PostgreSQL <c>TRUNCATE</c> statement that truncates all registered tables,
    /// restarting all identity sequences and cascading to dependent tables.
    /// Returns an empty string when no tables have been registered.
    /// </summary>
    /// <returns>
    /// A SQL string of the form
    /// <c>TRUNCATE TABLE table1, table2 RESTART IDENTITY CASCADE</c>,
    /// or <see cref="string.Empty"/> when no tables are registered.
    /// </returns>
    public string GetTruncateScript()
    {
        if (_tables.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder("TRUNCATE TABLE ");
        sb.AppendJoin(", ", _tables);
        sb.Append(" RESTART IDENTITY CASCADE");

        return sb.ToString();
    }

    /// <summary>
    /// Removes all registered table names, resetting this instance to its initial state.
    /// </summary>
    public void Clear() => _tables.Clear();
}
