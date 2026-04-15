using System.Text.Json;
using MarcusPrado.Platform.Abstractions.Context;
using MarcusPrado.Platform.Domain.Auditing;
using MarcusPrado.Platform.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MarcusPrado.Platform.EfCore.Tests.Helpers;

/// <summary>Minimal <see cref="AppDbContextBase"/> for unit tests.</summary>
internal sealed class TestDbContext : AppDbContextBase
{
    /// <summary>Auditable entities tracked in this context.</summary>
    public DbSet<AuditableTestEntity> AuditableEntities => Set<AuditableTestEntity>();

    /// <summary>Domain-event-raising entities tracked in this context.</summary>
    public DbSet<DomainEventTestEntity> DomainEventEntities => Set<DomainEventTestEntity>();

    /// <summary>Creates a <see cref="TestDbContext"/> with InMemory provider.</summary>
    public static TestDbContext CreateInMemory(string dbName = "test-db")
    {
        var opts = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(dbName).Options;
        return new TestDbContext(opts);
    }

    /// <summary>
    /// Creates a <see cref="TestDbContext"/> with InMemory provider and an optional
    /// <see cref="IDomainEventPublisher"/> for domain-event dispatch tests.
    /// </summary>
    public static TestDbContext CreateInMemory(string dbName, IDomainEventPublisher? publisher)
    {
        var opts = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(dbName).Options;
        return new TestDbContext(opts, publisher);
    }

    private TestDbContext(DbContextOptions<TestDbContext> options, IDomainEventPublisher? publisher = null)
        : base(options, publisher) { }

    /// <inheritdoc/>
    protected override void ConfigureModel(ModelBuilder modelBuilder)
    {
        // Map AuditRecord as a JSON-serialised scalar so that AppDbContextBase can
        // access it via entry.Property(nameof(IAuditable.Audit)).CurrentValue.
        var auditConverter = new ValueConverter<AuditRecord, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<AuditRecord>(v, (JsonSerializerOptions?)null)!
        );

        modelBuilder.Entity<AuditableTestEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Audit).HasConversion(auditConverter).IsRequired(false);
        });

        modelBuilder.Entity<DomainEventTestEntity>(e =>
        {
            e.HasKey(x => x.Id);
        });
    }
}

/// <summary>
/// Variant of <see cref="TestDbContext"/> that applies a tenant query filter.
/// Used for testing <c>TenantDbContextDecorator.ApplyTenantFilter</c>.
/// </summary>
internal sealed class TenantTestDbContext : AppDbContextBase
{
    private readonly ITenantContext _tenantContext;

    public DbSet<TenantTestEntity> TenantEntities => Set<TenantTestEntity>();

    public static TenantTestDbContext CreateInMemory(string dbName, ITenantContext tenantContext)
    {
        var opts = new DbContextOptionsBuilder<TenantTestDbContext>().UseInMemoryDatabase(dbName).Options;
        return new TenantTestDbContext(opts, tenantContext);
    }

    private TenantTestDbContext(DbContextOptions<TenantTestDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    /// <inheritdoc/>
    protected override void ConfigureModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantTestEntity>(e =>
        {
            e.HasKey(x => x.Id);
        });

        modelBuilder.ApplyTenantFilter(_tenantContext);
    }
}

/// <summary>Simple in-memory tenant context for tests.</summary>
internal sealed class FakeTenantContext : ITenantContext
{
    public string? TenantId { get; private set; }

    public FakeTenantContext(string? tenantId = null) => TenantId = tenantId;

    public void SetTenantId(string? tenantId) => TenantId = tenantId;
}
