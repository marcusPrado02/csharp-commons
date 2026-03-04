namespace MarcusPrado.Platform.MultiTenancy.Strategy;

/// <summary>
/// Isolates tenants using separate PostgreSQL schemas.
/// Each tenant has their own schema (e.g. <c>tenant-abc</c>), and
/// the search path is switched at connection time.
/// </summary>
public sealed class SchemaPerTenantStrategy : ITenantIsolationStrategy
{
    /// <inheritdoc />
    public string StrategyName => "SchemaPerTenant";

    /// <summary>Returns the schema name to use for the given tenant.</summary>
    public static string GetSchemaName(string tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        return tenantId.ToLowerInvariant().Replace("-", "_");
    }

    /// <summary>Returns the <c>SET search_path = ...</c> SQL command for the tenant.</summary>
    public static string BuildSearchPathSql(string tenantId)
        => $"SET search_path = \"{GetSchemaName(tenantId)}\", public;";
}
