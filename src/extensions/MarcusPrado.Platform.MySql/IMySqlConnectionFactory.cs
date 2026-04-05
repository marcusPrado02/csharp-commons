namespace MarcusPrado.Platform.MySql;

/// <summary>Factory for creating open MySQL database connections.</summary>
public interface IMySqlConnectionFactory
{
    /// <summary>Creates and opens a new MySQL connection.</summary>
    Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
