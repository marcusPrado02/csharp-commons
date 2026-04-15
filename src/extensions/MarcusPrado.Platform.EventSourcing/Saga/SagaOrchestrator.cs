namespace MarcusPrado.Platform.EventSourcing.Saga;

/// <summary>
/// Executes the steps of an <see cref="ISaga{TState}"/> in order and, on
/// failure, triggers compensations via a <see cref="SagaCompensationHandler"/>
/// in reverse order.
/// </summary>
public sealed class SagaOrchestrator
{
    /// <summary>
    /// Initialises a new instance of <see cref="SagaOrchestrator"/>.
    /// </summary>
    public SagaOrchestrator() { }

    /// <summary>
    /// Runs the saga to completion or triggers compensation on the first failure.
    /// </summary>
    /// <typeparam name="TState">The saga state type.</typeparam>
    /// <param name="saga">The saga instance to execute.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="SagaExecutionException">
    /// Thrown when a step fails. The inner exception contains the original error.
    /// </exception>
#pragma warning disable CA1822, S2325 // instance method intentional for DI/extensibility
    public async Task ExecuteAsync<TState>(ISaga<TState> saga, CancellationToken ct = default)
#pragma warning restore CA1822, S2325
    {
        saga.Status = SagaStatus.Running;

        var handler = new SagaCompensationHandler();

        foreach (var step in saga.Steps)
        {
            try
            {
                await step.ExecuteAsync(ct).ConfigureAwait(false);

                if (step.HasCompensation)
                {
                    handler.Register(c => step.CompensateAsync(c));
                }
            }
            catch (Exception ex)
            {
                saga.Status = SagaStatus.Compensating;

                await handler.CompensateAsync(ct).ConfigureAwait(false);

                saga.Status = SagaStatus.Failed;

                throw new SagaExecutionException($"Saga '{saga.Id}' failed at step '{step.Name}'.", step.Name, ex);
            }
        }

        saga.Status = SagaStatus.Completed;
    }
}
