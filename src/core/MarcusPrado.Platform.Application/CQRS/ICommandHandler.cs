using MarcusPrado.Platform.Abstractions.Results;

namespace MarcusPrado.Platform.Application.CQRS;

/// <summary>Handles a void <typeparamref name="TCommand"/>.</summary>
/// <typeparam name="TCommand">The concrete command type.</typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>Executes the command and returns a <see cref="Result"/>.</summary>
    Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>Handles a valued <typeparamref name="TCommand"/> producing a <typeparamref name="TResult"/>.</summary>
/// <typeparam name="TCommand">The concrete command type.</typeparam>
/// <typeparam name="TResult">The type of the value produced on success.</typeparam>
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    /// <summary>Executes the command and returns a <see cref="Result{TResult}"/>.</summary>
    Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
