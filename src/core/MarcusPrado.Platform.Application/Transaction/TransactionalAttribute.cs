namespace MarcusPrado.Platform.Application.Transaction;

/// <summary>
/// Marks a command as requiring an explicit database transaction.
/// The <see cref="MarcusPrado.Platform.Application.Pipeline.TransactionBehavior{TRequest,TResponse}"/>
/// wraps the handler invocation in <c>IUnitOfWork.BeginTransactionAsync()</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class TransactionalAttribute : Attribute
{
}
