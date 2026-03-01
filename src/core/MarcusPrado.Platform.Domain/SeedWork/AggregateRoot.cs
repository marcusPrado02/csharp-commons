using MarcusPrado.Platform.Domain.Events;

namespace MarcusPrado.Platform.Domain.SeedWork;

/// <summary>
/// Base class for aggregate roots.
/// Extends <see cref="Entity{TId}"/> with optimistic-concurrency versioning and
/// implements <see cref="IDomainEventRecorder"/> so infrastructure can harvest
/// domain events at the end of each unit of work.
/// </summary>
/// <typeparam name="TId">Strongly-typed aggregate identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>, IDomainEventRecorder
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Monotonically-increasing version counter used for optimistic concurrency control.
    /// Incremented by <see cref="IncrementVersion"/> inside command handlers.
    /// Persisted alongside the aggregate state and checked on save.
    /// </summary>
    public int Version { get; private set; }

    /// <summary>Initialises the aggregate root with its identifier at version 0.</summary>
    protected AggregateRoot(TId id)
        : base(id) { }

    // ── IDomainEventRecorder ─────────────────────────────────────────────────

    IReadOnlyCollection<IDomainEvent> IDomainEventRecorder.DomainEvents => DomainEvents;

    void IDomainEventRecorder.ClearDomainEvents() => ClearDomainEvents();

    // ── Versioning ────────────────────────────────────────────────────────────

    /// <summary>
    /// Bumps the in-memory version. Call once per successful state-changing command
    /// so that the persistence layer can detect concurrent modifications and throw
    /// <c>OptimisticConcurrencyException</c> before committing.
    /// </summary>
    protected void IncrementVersion() => Version++;

    /// <summary>
    /// Restores the version from persistence (used by repositories / event-sourcing replays).
    /// </summary>
    internal void RestoreVersion(int version) => Version = version;
}
