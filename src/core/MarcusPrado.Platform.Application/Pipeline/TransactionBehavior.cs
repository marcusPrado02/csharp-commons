using MarcusPrado.Platform.Abstractions.Storage;
using MarcusPrado.Platform.Application.Transaction;
using Microsoft.Extensions.Logging;

namespace MarcusPrado.Platform.Application.Pipeline;

/// <summary>
/// Wraps handler execution in an <see cref="IUnitOfWork"/> transaction for commands
/// decorated with <see cref="TransactionalAttribute"/>.
/// Commits on success, rolls back and logs on failure.
/// Registered as the seventh behavior (order 7).
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork? _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    /// <summary>Initializes the behavior.</summary>
    public TransactionBehavior(
        ILogger<TransactionBehavior<TRequest, TResponse>> logger,
        IUnitOfWork? unitOfWork = null)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        var hasAttribute = typeof(TRequest)
            .GetCustomAttributes(typeof(TransactionalAttribute), inherit: false)
            .Length > 0;

        if (!hasAttribute || _unitOfWork is null)
        {
            return await next(cancellationToken);
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            return response;
        }
#pragma warning disable S2139
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Transaction rolled back for {RequestType}",
                typeof(TRequest).Name);

            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
#pragma warning restore S2139
    }
}
