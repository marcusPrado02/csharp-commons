namespace MarcusPrado.Platform.EventSourcing.Projections;

/// <summary>Projects a domain event into a read model update.</summary>
public interface IProjection<in TEvent, TReadModel>
    where TEvent : IDomainEvent
    where TReadModel : class
{
    Task ApplyAsync(TEvent domainEvent, TReadModel readModel, CancellationToken cancellationToken = default);
}
