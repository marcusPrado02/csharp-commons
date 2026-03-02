using MarcusPrado.Platform.Abstractions.Results;

namespace MarcusPrado.Platform.Application.CQRS;

/// <summary>Handles a <typeparamref name="TQuery"/> and returns a <typeparamref name="TResult"/>.</summary>
/// <typeparam name="TQuery">The concrete query type.</typeparam>
/// <typeparam name="TResult">The projected result type.</typeparam>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    /// <summary>Executes the query.</summary>
    Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
