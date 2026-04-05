namespace MarcusPrado.Platform.EventSourcing.Saga;

/// <summary>
/// A default, mutable implementation of <see cref="ISaga{TState}"/> that
/// callers can construct and populate with <see cref="ISagaStepDescriptor"/> instances.
/// </summary>
/// <typeparam name="TState">The state type carried by this saga.</typeparam>
public sealed class DefaultSaga<TState> : ISaga<TState>
{
    private readonly List<ISagaStepDescriptor> _steps = [];

    /// <inheritdoc/>
    public string Id { get; }

    /// <inheritdoc/>
    public TState State { get; }

    /// <inheritdoc/>
    public SagaStatus Status { get; set; } = SagaStatus.Running;

    /// <inheritdoc/>
    public IReadOnlyList<ISagaStepDescriptor> Steps => _steps;

    /// <summary>
    /// Initialises a new <see cref="DefaultSaga{TState}"/>.
    /// </summary>
    /// <param name="id">The unique saga identifier.</param>
    /// <param name="state">The initial saga state.</param>
    public DefaultSaga(string id, TState state)
    {
        Id = id;
        State = state;
    }

    /// <summary>
    /// Adds a <see cref="BoundSagaStep{TCommand}"/> to this saga.
    /// </summary>
    /// <typeparam name="TCommand">The command type for the step.</typeparam>
    /// <param name="step">The step definition.</param>
    /// <param name="command">The command instance bound to the step.</param>
    public void AddStep<TCommand>(SagaStep<TCommand> step, TCommand command)
        => _steps.Add(new BoundSagaStep<TCommand>(step, command));
}
