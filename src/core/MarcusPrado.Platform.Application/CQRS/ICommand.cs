namespace MarcusPrado.Platform.Application.CQRS;

/// <summary>Marker for void commands that return <see cref="MarcusPrado.Platform.Abstractions.Results.Result"/>.</summary>
public interface ICommand { }

/// <summary>Marker for valued commands that return <see cref="MarcusPrado.Platform.Abstractions.Results.Result{TResult}"/>.</summary>
/// <typeparam name="TResult">The type of the value produced on success.</typeparam>
#pragma warning disable S2326 // TResult unused in interface — it is a phantom type that carries the result type through the type system
public interface ICommand<TResult> : ICommand { }
#pragma warning restore S2326
