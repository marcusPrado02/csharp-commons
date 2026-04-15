namespace MarcusPrado.Platform.EventSourcing.Saga;

/// <summary>
/// Executes registered compensation actions in reverse (LIFO) order.
/// Compensations are collected as the saga progresses and replayed backwards
/// when a failure occurs.
/// </summary>
public sealed class SagaCompensationHandler
{
    private readonly List<Func<CancellationToken, Task>> _compensations = [];

    /// <summary>
    /// Registers a compensation action to be run during rollback.
    /// </summary>
    /// <param name="compensate">The compensation delegate.</param>
    public void Register(Func<CancellationToken, Task> compensate) => _compensations.Add(compensate);

    /// <summary>
    /// Executes all registered compensations in reverse order.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    public async Task CompensateAsync(CancellationToken ct = default)
    {
        for (int i = _compensations.Count - 1; i >= 0; i--)
        {
            await _compensations[i](ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Gets the number of registered compensations.
    /// </summary>
    public int Count => _compensations.Count;
}
