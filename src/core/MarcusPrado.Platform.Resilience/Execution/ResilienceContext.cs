namespace MarcusPrado.Platform.Resilience.Execution;

/// <summary>
/// Per-execution context passed through the resilience pipeline.
/// Can be used to carry correlation IDs, tenant info, or custom state.
/// </summary>
public sealed class ResilienceContext
{
    /// <summary>Gets or sets the execution identifier (for logging/tracing).</summary>
    public string? OperationKey { get; init; }

    /// <summary>Gets or sets free-form properties attached to this execution.</summary>
    public IDictionary<string, object?> Properties { get; init; } =
        new Dictionary<string, object?>(StringComparer.Ordinal);

    /// <summary>Creates a default context with an optional operation key.</summary>
    public static ResilienceContext Create(string? operationKey = null) => new() { OperationKey = operationKey };
}
