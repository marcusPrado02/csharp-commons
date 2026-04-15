namespace MarcusPrado.Platform.EventSourcing.Projections;

/// <summary>Projects a domain event into a read model update.</summary>
public interface IProjection<in TEvent, TReadModel>
    where TEvent : IDomainEvent
    where TReadModel : class
{
    /// <summary>Applies the domain event to the read model, mutating it in place.</summary>
    /// <param name="domainEvent">The domain event to project.</param>
    /// <param name="readModel">The read model to update.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task ApplyAsync(TEvent domainEvent, TReadModel readModel, CancellationToken cancellationToken = default);
}
