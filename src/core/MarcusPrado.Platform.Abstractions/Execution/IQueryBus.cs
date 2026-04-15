using MarcusPrado.Platform.Abstractions.Results;

namespace MarcusPrado.Platform.Abstractions.Execution;

/// <summary>Dispatches queries through the CQRS read pipeline.</summary>
public interface IQueryBus
{
    /// <summary>Executes a query and returns its typed result.</summary>
    Task<Result<TResult>> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : notnull;
}
