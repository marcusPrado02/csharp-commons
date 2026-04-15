namespace MarcusPrado.Platform.Abstractions.Execution;

/// <summary>
/// Combined dispatcher that exposes both command and query dispatch.
/// The concrete implementation lives in <c>MarcusPrado.Platform.Application</c>.
/// </summary>
public interface IDispatcher : ICommandBus, IQueryBus { }
