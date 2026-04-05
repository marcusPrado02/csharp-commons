namespace MarcusPrado.Platform.EventSourcing.Saga;

/// <summary>
/// Represents a single step in a saga, wrapping a command execution
/// with an optional compensation action and an optional timeout.
/// </summary>
/// <typeparam name="TCommand">The command type processed by this step.</typeparam>
public sealed class SagaStep<TCommand>
{
    /// <summary>Gets the human-readable name for this step.</summary>
    public string Name { get; }

    /// <summary>Gets the function that executes the step's main action.</summary>
    public Func<TCommand, CancellationToken, Task> Execute { get; }

    /// <summary>Gets the optional compensation function to undo this step's action.</summary>
    public Func<TCommand, CancellationToken, Task>? Compensate { get; }

    /// <summary>Gets the optional timeout for the execute action.</summary>
    public TimeSpan? Timeout { get; }

    /// <summary>
    /// Initialises a new <see cref="SagaStep{TCommand}"/>.
    /// </summary>
    /// <param name="name">The step name.</param>
    /// <param name="execute">The execution delegate.</param>
    /// <param name="compensate">The optional compensation delegate.</param>
    /// <param name="timeout">The optional per-step timeout.</param>
    public SagaStep(
        string name,
        Func<TCommand, CancellationToken, Task> execute,
        Func<TCommand, CancellationToken, Task>? compensate = null,
        TimeSpan? timeout = null)
    {
        Name = name;
        Execute = execute;
        Compensate = compensate;
        Timeout = timeout;
    }
}
