namespace MyDomain.Domain.Entities;

/// <summary>Base entity with domain events support.</summary>
public abstract class Entity
{
    private readonly List<object> _domainEvents = new();

    /// <summary>Gets the unique identifier.</summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>Gets the domain events raised by this entity.</summary>
    public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Adds a domain event.</summary>
    protected void AddDomainEvent(object domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>Clears all domain events.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
