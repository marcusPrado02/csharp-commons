namespace MarcusPrado.Platform.Application.Pipeline;

/// <summary>
/// Delegate that calls the next step (or handler) in the pipeline.
/// </summary>
/// <typeparam name="TResponse">The response type.</typeparam>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(
    CancellationToken cancellationToken = default);

/// <summary>
/// A single cross-cutting concern that wraps command or query handling.
/// Behaviors are executed in registration order; each calls the next delegate
/// to continue the chain.
/// </summary>
/// <typeparam name="TRequest">The request (command or query) type.</typeparam>
/// <typeparam name="TResponse">The response type; typically <c>Result</c> or <c>Result&lt;T&gt;</c>.</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
{
    /// <summary>Handles the request step and delegates to <paramref name="next"/>.</summary>
    Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default);
}
