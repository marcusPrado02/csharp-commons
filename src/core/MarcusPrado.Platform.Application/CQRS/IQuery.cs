namespace MarcusPrado.Platform.Application.CQRS;

/// <summary>
/// Marker for read-only queries that return
/// <see cref="MarcusPrado.Platform.Abstractions.Results.Result{TResult}"/>.
/// </summary>
/// <typeparam name="TResult">The type of the projected response.</typeparam>
#pragma warning disable S2326 // TResult unused in interface — phantom type that binds handler to result type
public interface IQuery<TResult>
{
}
#pragma warning restore S2326
