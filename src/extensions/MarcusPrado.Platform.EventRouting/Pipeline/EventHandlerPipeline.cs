using MarcusPrado.Platform.Domain.Events;

namespace MarcusPrado.Platform.EventRouting.Pipeline;

/// <summary>
/// A composable pipeline of middlewares that wrap domain event handler execution.
/// Middlewares are invoked in registration order; each middleware receives the event
/// and a <c>next</c> delegate it must call to continue the chain.
/// </summary>
public sealed class EventHandlerPipeline
{
    private readonly List<Func<IDomainEvent, Func<Task>, Task>> _middlewares = [];

    /// <summary>
    /// Adds a middleware to the end of the pipeline.
    /// </summary>
    /// <param name="middleware">
    /// A delegate that receives the current <see cref="IDomainEvent"/> and a <c>next</c>
    /// delegate. The middleware must call <c>next()</c> to continue processing.
    /// </param>
    /// <returns>The current <see cref="EventHandlerPipeline"/> for fluent chaining.</returns>
    public EventHandlerPipeline Use(Func<IDomainEvent, Func<Task>, Task> middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        _middlewares.Add(middleware);
        return this;
    }

    /// <summary>
    /// Executes the full pipeline for the given <paramref name="domainEvent"/>,
    /// ultimately invoking <paramref name="handler"/> at the end of the chain.
    /// </summary>
    /// <param name="domainEvent">The event being processed.</param>
    /// <param name="handler">The terminal action representing the actual handler(s).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public Task ExecuteAsync(
        IDomainEvent domainEvent,
        Func<Task> handler,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        ArgumentNullException.ThrowIfNull(handler);

        // Build the chain from the inside out.
        Func<Task> current = handler;
        for (int i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var next = current;
            var captured = domainEvent;
            current = () => middleware(captured, next);
        }

        return current();
    }

    /// <summary>Returns the number of middlewares currently registered in the pipeline.</summary>
    public int Count => _middlewares.Count;
}
