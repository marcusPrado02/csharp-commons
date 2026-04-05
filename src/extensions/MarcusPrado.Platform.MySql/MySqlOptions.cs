namespace MarcusPrado.Platform.MySql;

/// <summary>Configuration options for the MySQL platform extension.</summary>
public sealed class MySqlOptions
{
    /// <summary>The MySQL connection string.</summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>MySQL server version string, e.g. "8.0.33". Used by Pomelo.</summary>
    public string ServerVersion { get; set; } = "8.0.33";

    /// <summary>Maximum number of retry attempts on transient failures.</summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>Maximum delay between retry attempts.</summary>
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(5);
}
