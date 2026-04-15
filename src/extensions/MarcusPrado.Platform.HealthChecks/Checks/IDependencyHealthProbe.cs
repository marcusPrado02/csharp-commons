namespace MarcusPrado.Platform.HealthChecks.Checks;

/// <summary>
/// Abstraction for a single dependency probe (database, cache, broker, etc.)
/// used by <see cref="ReadinessCheck"/> to determine service readiness.
/// </summary>
public interface IDependencyHealthProbe
{
    /// <summary>Gets the human-readable probe name (e.g. "postgres", "redis").</summary>
    string Name { get; }

    /// <summary>
    /// Checks whether the dependency is available.
    /// </summary>
    /// <returns>A tuple of (healthy, message) where message describes the result.</returns>
    Task<(bool Healthy, string Message)> CheckAsync(CancellationToken cancellationToken = default);
}
