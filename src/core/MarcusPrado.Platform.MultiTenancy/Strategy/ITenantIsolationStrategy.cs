namespace MarcusPrado.Platform.MultiTenancy.Strategy;

/// <summary>Defines how tenant data is physically isolated.</summary>
public interface ITenantIsolationStrategy
{
    /// <summary>Unique name identifying this isolation strategy.</summary>
    string StrategyName { get; }
}
