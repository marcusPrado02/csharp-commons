using MarcusPrado.Platform.Abstractions.Results;

namespace MarcusPrado.Platform.Abstractions.Execution;

/// <summary>Dispatches commands through the CQRS pipeline.</summary>
public interface ICommandBus
{
    /// <summary>Sends a void command (no return value) through the pipeline.</summary>
    Task<Result> SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : notnull;

    /// <summary>Sends a valued command through the pipeline.</summary>
    Task<Result<TResult>> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : notnull;
}
