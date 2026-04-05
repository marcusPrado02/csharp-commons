namespace MarcusPrado.Platform.EventSourcing.Saga;

/// <summary>
/// Binds a <see cref="SagaStep{TCommand}"/> to a concrete command instance,
/// implementing <see cref="ISagaStepDescriptor"/> so the orchestrator can invoke
/// it without knowing the generic type parameter.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
public sealed class BoundSagaStep<TCommand> : ISagaStepDescriptor
{
    private readonly SagaStep<TCommand> _step;
    private readonly TCommand _command;

    /// <summary>
    /// Initialises a new <see cref="BoundSagaStep{TCommand}"/>.
    /// </summary>
    /// <param name="step">The step definition.</param>
    /// <param name="command">The command instance to bind.</param>
    public BoundSagaStep(SagaStep<TCommand> step, TCommand command)
    {
        _step = step;
        _command = command;
    }

    /// <inheritdoc/>
    public string Name => _step.Name;

    /// <inheritdoc/>
    public TimeSpan? Timeout => _step.Timeout;

    /// <inheritdoc/>
    public bool HasCompensation => _step.Compensate is not null;

    /// <inheritdoc/>
    public async Task ExecuteAsync(CancellationToken ct)
    {
        if (_step.Timeout.HasValue)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(_step.Timeout.Value);
            await _step.Execute(_command, cts.Token).ConfigureAwait(false);
        }
        else
        {
            await _step.Execute(_command, ct).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public Task CompensateAsync(CancellationToken ct)
    {
        if (_step.Compensate is null)
        {
            return Task.CompletedTask;
        }

        return _step.Compensate(_command, ct);
    }
}
