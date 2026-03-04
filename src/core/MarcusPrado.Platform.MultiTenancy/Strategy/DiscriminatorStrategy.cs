namespace MarcusPrado.Platform.MultiTenancy.Strategy;

/// <summary>
/// Isolates tenants using a discriminator column (<c>TenantId</c>) in a shared database.
/// Works with EF Core global query filters.
/// </summary>
public sealed class DiscriminatorStrategy : ITenantIsolationStrategy
{
    /// <inheritdoc />
    public string StrategyName => "Discriminator";

    /// <summary>The column name used as discriminator (default: <c>TenantId</c>).</summary>
    public string DiscriminatorColumn { get; init; } = "TenantId";
}
