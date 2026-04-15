namespace MarcusPrado.Platform.MultiTenancy.Strategy;

/// <summary>
/// Isolates tenants in separate databases.
/// The connection string for each tenant is resolved at runtime.
/// </summary>
public sealed class DatabasePerTenantStrategy : ITenantIsolationStrategy
{
    private readonly Dictionary<string, string> _connections;

    /// <summary>Initialises the strategy, optionally pre-seeding it with a map of tenant IDs to connection strings.</summary>
    public DatabasePerTenantStrategy(IReadOnlyDictionary<string, string>? connectionStrings = null)
    {
        _connections = connectionStrings is not null
            ? new Dictionary<string, string>(connectionStrings, StringComparer.OrdinalIgnoreCase)
            : [];
    }

    /// <inheritdoc />
    public string StrategyName => "DatabasePerTenant";

    /// <summary>Registers a connection string for a tenant.</summary>
    public void RegisterTenant(string tenantId, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        _connections[tenantId] = connectionString;
    }

    /// <summary>Resolves the connection string for a tenant.</summary>
    public string? GetConnectionString(string tenantId) => _connections.TryGetValue(tenantId, out var cs) ? cs : null;
}
