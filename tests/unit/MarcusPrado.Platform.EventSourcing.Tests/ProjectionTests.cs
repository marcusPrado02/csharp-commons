using FluentAssertions;
using MarcusPrado.Platform.Domain.Events;
using MarcusPrado.Platform.EventSourcing;
using MarcusPrado.Platform.EventSourcing.Projections;
using Xunit;

namespace MarcusPrado.Platform.EventSourcing.Tests;

// ── Test domain types ─────────────────────────────────────────────────────────

internal sealed class ProductCreatedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public string EventType => nameof(ProductCreatedEvent);
    public string ProductId { get; init; } = "";
    public string Name { get; init; } = "";
}

internal sealed class ProductViewedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public string EventType => nameof(ProductViewedEvent);
    public string ProductId { get; init; } = "";
}

internal sealed class ProductReadModel
{
    public string Name { get; set; } = "";
    public int ViewCount { get; set; }
}

internal sealed class ProductCreatedProjection : IProjection<ProductCreatedEvent, ProductReadModel>
{
    public Task ApplyAsync(ProductCreatedEvent evt, ProductReadModel model, CancellationToken ct = default)
    {
        model.Name = evt.Name;
        return Task.CompletedTask;
    }
}

internal sealed class ProductViewedProjection : IProjection<ProductViewedEvent, ProductReadModel>
{
    public Task ApplyAsync(ProductViewedEvent evt, ProductReadModel model, CancellationToken ct = default)
    {
        model.ViewCount++;
        return Task.CompletedTask;
    }
}

// Second projection for ProductCreatedEvent — used in multi-projection test
internal sealed class ProductNameUpperCaseProjection : IProjection<ProductCreatedEvent, ProductReadModel>
{
    public Task ApplyAsync(ProductCreatedEvent evt, ProductReadModel model, CancellationToken ct = default)
    {
        model.Name = evt.Name.ToUpperInvariant();
        return Task.CompletedTask;
    }
}

// ── Tests ─────────────────────────────────────────────────────────────────────

public sealed class InMemoryReadModelStoreTests
{
    [Fact]
    public async Task GetAsync_MissingKey_ReturnsNull()
    {
        var store = new InMemoryReadModelStore<ProductReadModel>();

        var result = await store.GetAsync("nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_ThenGetAsync_ReturnsStoredModel()
    {
        var store = new InMemoryReadModelStore<ProductReadModel>();
        var model = new ProductReadModel { Name = "Widget", ViewCount = 5 };

        await store.SaveAsync("p-1", model);
        var result = await store.GetAsync("p-1");

        result.Should().NotBeNull();
        result!.Name.Should().Be("Widget");
        result.ViewCount.Should().Be(5);
    }

    [Fact]
    public async Task DeleteAsync_RemovesEntry()
    {
        var store = new InMemoryReadModelStore<ProductReadModel>();
        await store.SaveAsync("p-2", new ProductReadModel { Name = "ToDelete" });

        await store.DeleteAsync("p-2");
        var result = await store.GetAsync("p-2");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllStoredItems()
    {
        var store = new InMemoryReadModelStore<ProductReadModel>();
        await store.SaveAsync("p-1", new ProductReadModel { Name = "A" });
        await store.SaveAsync("p-2", new ProductReadModel { Name = "B" });
        await store.SaveAsync("p-3", new ProductReadModel { Name = "C" });

        var all = await store.GetAllAsync();

        all.Should().HaveCount(3);
        all.Select(m => m.Name).Should().BeEquivalentTo(new[] { "A", "B", "C" });
    }
}

public sealed class ProjectionEngineTests
{
    [Fact]
    public async Task DispatchAsync_WithRegisteredProjection_ProjectionApplied()
    {
        var store = new InMemoryReadModelStore<ProductReadModel>();
        var engine = new ProjectionEngine();
        engine.Register(new ProductCreatedProjection(), store, e => e.ProductId);

        await engine.DispatchAsync(new ProductCreatedEvent { ProductId = "p-1", Name = "Gadget" });

        var model = await store.GetAsync("p-1");
        model.Should().NotBeNull();
        model!.Name.Should().Be("Gadget");
    }

    [Fact]
    public async Task DispatchAsync_NoRegisteredProjection_NoError()
    {
        var engine = new ProjectionEngine();

        // Dispatching an event with no registered handler should not throw
        var act = () => engine.DispatchAsync(new ProductCreatedEvent { ProductId = "p-1", Name = "X" });

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Register_DispatchAsync_CreatesNewModelWhenNoneExists()
    {
        var store = new InMemoryReadModelStore<ProductReadModel>();
        var engine = new ProjectionEngine();
        engine.Register(new ProductCreatedProjection(), store, e => e.ProductId);

        // No pre-existing model for "new-product"
        await engine.DispatchAsync(new ProductCreatedEvent { ProductId = "new-product", Name = "Brand New" });

        var model = await store.GetAsync("new-product");
        model.Should().NotBeNull();
        model!.Name.Should().Be("Brand New");
    }

    [Fact]
    public async Task Register_DispatchAsync_UpdatesExistingModel()
    {
        var store = new InMemoryReadModelStore<ProductReadModel>();
        var engine = new ProjectionEngine();
        engine.Register(new ProductViewedProjection(), store, e => e.ProductId);

        // Pre-seed the model
        await store.SaveAsync("p-5", new ProductReadModel { Name = "Existing", ViewCount = 2 });

        await engine.DispatchAsync(new ProductViewedEvent { ProductId = "p-5" });

        var model = await store.GetAsync("p-5");
        model.Should().NotBeNull();
        model!.ViewCount.Should().Be(3);
        model.Name.Should().Be("Existing"); // unchanged field
    }

    [Fact]
    public async Task Register_MultipleProjectionsForSameEvent_BothApplied()
    {
        var store = new InMemoryReadModelStore<ProductReadModel>();
        var engine = new ProjectionEngine();

        // First projection sets Name, second sets Name to uppercase
        engine.Register(new ProductCreatedProjection(), store, e => e.ProductId);
        engine.Register(new ProductNameUpperCaseProjection(), store, e => e.ProductId);

        await engine.DispatchAsync(new ProductCreatedEvent { ProductId = "p-multi", Name = "hello" });

        var model = await store.GetAsync("p-multi");
        model.Should().NotBeNull();
        // Both projections ran; second one overwrote Name with uppercase
        model!.Name.Should().Be("HELLO");
    }
}

public sealed class ProjectionRebuildJobTests
{
    [Fact]
    public async Task RebuildAsync_ReplaysEventsAndAppliesProjections()
    {
        var eventStore = new InMemoryEventStore();
        var store = new InMemoryReadModelStore<ProductReadModel>();
        var engine = new ProjectionEngine();
        engine.Register(new ProductCreatedProjection(), store, e => e.ProductId);

        // Append a ProductCreatedEvent to the event store
        await eventStore.AppendAsync(
            "product-stream",
            new IDomainEvent[] { new ProductCreatedEvent { ProductId = "p-rebuild", Name = "Rebuilt Product" } },
            expectedVersion: -1);

        var job = new ProjectionRebuildJob(eventStore, engine);
        await job.RebuildAsync("product-stream");

        var model = await store.GetAsync("p-rebuild");
        model.Should().NotBeNull();
        model!.Name.Should().Be("Rebuilt Product");
    }

    [Fact]
    public async Task RebuildAsync_MultipleEvents_AllApplied()
    {
        var eventStore = new InMemoryEventStore();
        var store = new InMemoryReadModelStore<ProductReadModel>();
        var engine = new ProjectionEngine();
        engine.Register(new ProductViewedProjection(), store, e => e.ProductId);

        await eventStore.AppendAsync(
            "view-stream",
            new IDomainEvent[]
            {
                new ProductViewedEvent { ProductId = "p-v" },
                new ProductViewedEvent { ProductId = "p-v" },
                new ProductViewedEvent { ProductId = "p-v" },
            },
            expectedVersion: -1);

        var job = new ProjectionRebuildJob(eventStore, engine);
        await job.RebuildAsync("view-stream");

        var model = await store.GetAsync("p-v");
        model.Should().NotBeNull();
        model!.ViewCount.Should().Be(3);
    }
}
