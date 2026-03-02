namespace MarcusPrado.Platform.Postgres.Options;

/// <summary>Configuration options for the PostgreSQL platform extension.</summary>
public sealed class PostgresOptions
{
    /// <summary>Gets or sets the Npgsql connection string.</summary>
    public string ConnectionString { get; set; } = "Host=localhost;Database=platform;Username=platform";

    /// <summary>When true, uses snake_case column/table naming.</summary>
    public bool UseSnakeCase { get; set; } = true;

    /// <summary>Gets or sets the connection pool size.</summary>
    public int MaxPoolSize { get; set; } = 20;

    /// <summary>Gets or sets the command timeout in seconds.</summary>
    public int CommandTimeoutSeconds { get; set; } = 30;
}
