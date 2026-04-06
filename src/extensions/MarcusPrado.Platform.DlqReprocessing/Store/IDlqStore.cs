namespace MarcusPrado.Platform.DlqReprocessing.Store;

/// <summary>
/// Abstraction over a dead-letter queue store, broker-agnostic.
/// </summary>
public interface IDlqStore
{
    /// <summary>
    /// Returns all messages currently in the DLQ for the specified <paramref name="topic"/>.
    /// </summary>
    /// <param name="topic">The topic or queue name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A read-only list of <see cref="DlqMessage"/> instances.</returns>
    Task<IReadOnlyList<DlqMessage>> GetAsync(string topic, CancellationToken ct = default);

    /// <summary>
    /// Returns the message with the specified <paramref name="id"/> in <paramref name="topic"/>,
    /// or <see langword="null"/> if it does not exist.
    /// </summary>
    /// <param name="topic">The topic or queue name.</param>
    /// <param name="id">The message identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The matching <see cref="DlqMessage"/>, or <see langword="null"/>.</returns>
    Task<DlqMessage?> GetByIdAsync(string topic, string id, CancellationToken ct = default);

    /// <summary>
    /// Marks the message identified by <paramref name="id"/> in <paramref name="topic"/> for requeue.
    /// Implementations should remove it from the DLQ after signalling reprocessing.
    /// </summary>
    /// <param name="topic">The topic or queue name.</param>
    /// <param name="id">The message identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RequeueAsync(string topic, string id, CancellationToken ct = default);

    /// <summary>
    /// Permanently removes the message identified by <paramref name="id"/> from <paramref name="topic"/>.
    /// </summary>
    /// <param name="topic">The topic or queue name.</param>
    /// <param name="id">The message identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteAsync(string topic, string id, CancellationToken ct = default);

    /// <summary>
    /// Adds a new <see cref="DlqMessage"/> to the store.
    /// </summary>
    /// <param name="message">The message to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(DlqMessage message, CancellationToken ct = default);
}
