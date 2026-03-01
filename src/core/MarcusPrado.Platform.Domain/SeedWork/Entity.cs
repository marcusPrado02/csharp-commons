using MarcusPrado.Platform.Domain.Events;

namespace MarcusPrado.Platform.Domain.SeedWork;

/// <summary>
/// Base class for all domain entities.
/// Equality is identity-based: two entities are equal if and only if they share
/// the same <typeparamref name="TId"/> value — regardless of all other fields.
/// </summary>
/// <typeparam name="TId">
/// The strongly-typed identifier type. Must implement <see cref="IEquatable{T}"/>.
/// </typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : IEquatable<TId>
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>The unique identifier of this entity.</summary>
    public TId Id { get; }

    /// <summary>
    /// Domain events raised during the current unit of work.
    /// Cleared after they are dispatched (see <see cref="ClearDomainEvents"/>).
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Initialises a new entity with the supplied identifier.</summary>
    protected Entity(TId id)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        Id = id;
    }

    // ── Domain events ─────────────────────────────────────────────────────────

    /// <summary>Records a domain event to be dispatched at the end of the unit of work.</summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent, nameof(domainEvent));
        _domainEvents.Add(domainEvent);
    }

    /// <summary>Removes all recorded domain events. Called by infrastructure after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    // ── Business-rule guard ───────────────────────────────────────────────────

    /// <summary>
    /// Guards aggregate invariants.  Call inside command methods with an
    /// <see cref="IBusinessRule"/> instance; throws
    /// <see cref="BusinessRuleViolationException"/> when the rule is broken.
    /// </summary>
    protected static void CheckRule(IBusinessRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule, nameof(rule));
        if (rule.IsBroken())
            throw new BusinessRuleViolationException(rule);
    }

    // ── Identity-based equality ────────────────────────────────────────────────

    /// <inheritdoc/>
    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as Entity<TId>);

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(GetType(), EqualityComparer<TId>.Default.GetHashCode(Id!));

    /// <summary>Two entities are equal when their IDs are equal (identity equality).</summary>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => left?.Equals(right) ?? right is null;

    /// <summary>Two entities are not equal when their IDs differ.</summary>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        => !(left == right);

    /// <inheritdoc/>
    public override string ToString() => $"{GetType().Name}#{Id}";
}
