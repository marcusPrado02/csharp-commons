using MarcusPrado.Platform.Domain.Events;

namespace MarcusPrado.Platform.EfCore.DbContext;

/// <summary>
/// Entities that accumulate domain events during a unit of work implement
/// this interface so <see cref="AppDbContextBase"/> can collect and dispatch them.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>Domain events raised since the last <c>ClearDomainEvents</c> call.</summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>Clears all accumulated events after dispatch.</summary>
    void ClearDomainEvents();
}
