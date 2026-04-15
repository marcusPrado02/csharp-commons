using System.Reflection;
using MarcusPrado.Platform.Abstractions.Execution;
using MarcusPrado.Platform.Abstractions.Results;
using MarcusPrado.Platform.Application.CQRS;
using MarcusPrado.Platform.Application.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Application.Execution;

/// <summary>
/// Default implementation of <see cref="IDispatcher"/>.
/// Resolves handlers and pipeline behaviors from the DI container and wires
/// them into an ordered delegate chain before calling the handler.
/// </summary>
public sealed class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>Initializes the dispatcher with the application's service provider.</summary>
    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public Task<Result> SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : notnull
    {
        var commandType = command.GetType();
#pragma warning disable S3011
        var method = typeof(Dispatcher)
            .GetMethod(nameof(SendVoidCoreAsync), BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(commandType);
#pragma warning restore S3011

        return (Task<Result>)method.Invoke(this, new object[] { command, cancellationToken })!;
    }

    /// <inheritdoc/>
    public Task<Result<TResult>> SendAsync<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken = default
    )
        where TCommand : notnull
    {
        var commandType = command.GetType();
#pragma warning disable S3011
        var method = typeof(Dispatcher)
            .GetMethod(nameof(SendValuedCoreAsync), BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(commandType, typeof(TResult));
#pragma warning restore S3011

        return (Task<Result<TResult>>)method.Invoke(this, new object[] { command, cancellationToken })!;
    }

    /// <inheritdoc/>
    public Task<Result<TResult>> QueryAsync<TQuery, TResult>(
        TQuery query,
        CancellationToken cancellationToken = default
    )
        where TQuery : notnull
    {
        var queryType = query.GetType();
#pragma warning disable S3011
        var method = typeof(Dispatcher)
            .GetMethod(nameof(QueryCoreAsync), BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(queryType, typeof(TResult));
#pragma warning restore S3011

        return (Task<Result<TResult>>)method.Invoke(this, new object[] { query, cancellationToken })!;
    }

    // ── Private core dispatch ───────────────────────────────────────────────

    private Task<Result> SendVoidCoreAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
        where TCommand : ICommand
    {
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        var behaviors = _serviceProvider.GetServices<IPipelineBehavior<TCommand, Result>>().ToArray();

        RequestHandlerDelegate<Result> pipeline = ct => handler.HandleAsync(command, ct);

        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var next = pipeline;

            pipeline = ct => behavior.HandleAsync(command, next, ct);
        }

        return pipeline(cancellationToken);
    }

    private Task<Result<TResult>> SendValuedCoreAsync<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken
    )
        where TCommand : ICommand<TResult>
    {
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
        var behaviors = _serviceProvider.GetServices<IPipelineBehavior<TCommand, Result<TResult>>>().ToArray();

        RequestHandlerDelegate<Result<TResult>> pipeline = ct => handler.HandleAsync(command, ct);

        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var next = pipeline;

            pipeline = ct => behavior.HandleAsync(command, next, ct);
        }

        return pipeline(cancellationToken);
    }

    private Task<Result<TResult>> QueryCoreAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken)
        where TQuery : IQuery<TResult>
    {
        var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        var behaviors = _serviceProvider.GetServices<IPipelineBehavior<TQuery, Result<TResult>>>().ToArray();

        RequestHandlerDelegate<Result<TResult>> pipeline = ct => handler.HandleAsync(query, ct);

        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var next = pipeline;

            pipeline = ct => behavior.HandleAsync(query, next, ct);
        }

        return pipeline(cancellationToken);
    }
}
