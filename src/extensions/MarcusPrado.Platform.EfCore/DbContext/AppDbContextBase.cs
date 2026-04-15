using MarcusPrado.Platform.Abstractions.Storage;
using MarcusPrado.Platform.Domain.Auditing;
using MarcusPrado.Platform.Domain.Events;
using MarcusPrado.Platform.OutboxInbox.Inbox;
using MarcusPrado.Platform.OutboxInbox.Outbox;
using Microsoft.EntityFrameworkCore;

namespace MarcusPrado.Platform.EfCore.DbContext;

/// <summary>
/// Base <see cref="DbContext"/> that automatically fills audit records and
/// dispatches domain events after committing changes.
/// Extend this class for each bounded context.
/// </summary>
public abstract class AppDbContextBase : Microsoft.EntityFrameworkCore.DbContext
{
    private readonly IDomainEventPublisher? _eventPublisher;

    /// <summary>Outbox messages persisted in the same database transaction.</summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>Inbox messages persisted in the same database transaction.</summary>
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    /// <summary>Initialises the context with optional domain event dispatching.</summary>
    protected AppDbContextBase(DbContextOptions options, IDomainEventPublisher? eventPublisher = null)
        : base(options)
    {
        _eventPublisher = eventPublisher;
    }

    /// <inheritdoc/>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        FillAuditRecords();
        var domainEvents = CollectDomainEvents();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (_eventPublisher is not null && domainEvents.Count > 0)
        {
            await _eventPublisher.PublishAllAsync(domainEvents, cancellationToken);
        }

        return result;
    }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OutboxMessage>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasIndex(m => m.State);
            e.HasIndex(m => m.ScheduledAt);
        });

        modelBuilder.Entity<InboxMessage>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasIndex(m => m.State);
            e.HasIndex(m => m.MessageId).IsUnique();
        });

        ConfigureModel(modelBuilder);
    }

    /// <summary>Override to customise the model for the derived bounded context.</summary>
    protected virtual void ConfigureModel(ModelBuilder modelBuilder) { }

    // ── Private helpers ────────────────────────────────────────────────────

    private void FillAuditRecords()
    {
        const string SystemActor = "system";
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(nameof(IAuditable.Audit)).CurrentValue = AuditRecord.Create(SystemActor, now);
                    break;

                case EntityState.Modified:
                    var existing = (AuditRecord)entry.Property(nameof(IAuditable.Audit)).CurrentValue!;
                    entry.Property(nameof(IAuditable.Audit)).CurrentValue = existing.Update(SystemActor, now);
                    break;
            }
        }
    }

    private List<IDomainEvent> CollectDomainEvents()
    {
        var aggregates = ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        var events = aggregates.SelectMany(a => a.DomainEvents).ToList();
        aggregates.ForEach(a => a.ClearDomainEvents());
        return events;
    }
}
