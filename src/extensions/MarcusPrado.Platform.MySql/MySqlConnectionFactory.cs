using MySqlConnector;

namespace MarcusPrado.Platform.MySql;

/// <summary>Default implementation of <see cref="IMySqlConnectionFactory"/> using MySqlConnector.</summary>
public sealed class MySqlConnectionFactory : IMySqlConnectionFactory
{
    private readonly MySqlOptions _options;

    /// <summary>Initialises the factory with the provided options.</summary>
    public MySqlConnectionFactory(IOptions<MySqlOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
    }

    /// <inheritdoc/>
    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var conn = new MySqlConnection(_options.ConnectionString);
        await conn.OpenAsync(cancellationToken);
        return conn;
    }
}
