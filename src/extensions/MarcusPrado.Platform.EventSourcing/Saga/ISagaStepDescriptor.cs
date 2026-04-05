namespace MarcusPrado.Platform.EventSourcing.Saga;

/// <summary>
/// Non-generic descriptor of a saga step, used by the orchestrator
/// to execute steps without knowing the concrete command type at compile time.
/// </summary>
public interface ISagaStepDescriptor
{
    /// <summary>Gets the human-readable name for this step.</summary>
    string Name { get; }

    /// <summary>Gets the optional per-step execution timeout.</summary>
    TimeSpan? Timeout { get; }

    /// <summary>Returns whether this step has a compensation action.</summary>
    bool HasCompensation { get; }

    /// <summary>
    /// Executes the step's main action.
    /// </summary>
    /// <param name="ct">Cancellation token (may be replaced internally when a Timeout is set).</param>
    Task ExecuteAsync(CancellationToken ct);

    /// <summary>
    /// Executes the step's compensation action, if any.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task CompensateAsync(CancellationToken ct);
}
