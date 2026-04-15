using FluentAssertions;
using MarcusPrado.Platform.Contracts.Async;
using MarcusPrado.Platform.Domain.Events;
using MarcusPrado.Platform.EventRouting.Bridge;
using MarcusPrado.Platform.EventRouting.Extensions;
using MarcusPrado.Platform.EventRouting.Handling;
using MarcusPrado.Platform.EventRouting.Pipeline;
using MarcusPrado.Platform.EventRouting.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MarcusPrado.Platform.EventRouting.Tests;

// ---------------------------------------------------------------------------
// Test doubles
// ---------------------------------------------------------------------------

/// <summary>Minimal domain event for testing.</summary>
internal sealed record OrderPlacedEvent(Guid OrderId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public string EventType => "order.placed";
}

/// <summary>A second domain event type to verify isolation between types.</summary>
internal sealed record OrderCancelledEvent(Guid OrderId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public string EventType => "order.cancelled";
}

/// <summary>A simple event contract produced by the bridge.</summary>
internal sealed record OrderPlacedContract(Guid OrderId) : IEventContract;

/// <summary>Tracking handler that records all received events.</summary>
internal sealed class TrackingOrderHandler : IDomainEventHandler<OrderPlacedEvent>
{
    public List<OrderPlacedEvent> Received { get; } = [];

    public Task HandleAsync(OrderPlacedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Received.Add(domainEvent);
        return Task.CompletedTask;
    }
}

/// <summary>A second independent handler for the same event type.</summary>
internal sealed class SecondOrderHandler : IDomainEventHandler<OrderPlacedEvent>
{
    public List<OrderPlacedEvent> Received { get; } = [];

    public Task HandleAsync(OrderPlacedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Received.Add(domainEvent);
        return Task.CompletedTask;
    }
}

/// <summary>Tracking handler for the cancellation event.</summary>
internal sealed class TrackingCancelHandler : IDomainEventHandler<OrderCancelledEvent>
{
    public List<OrderCancelledEvent> Received { get; } = [];

    public Task HandleAsync(OrderCancelledEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Received.Add(domainEvent);
        return Task.CompletedTask;
    }
}

// ---------------------------------------------------------------------------
// Test suites
// ---------------------------------------------------------------------------

public sealed class EventHandlerPipelineTests
{
    [Fact]
    public async Task ExecuteAsync_WithNoMiddlewares_InvokesHandlerDirectly()
    {
        // Arrange
        var pipeline = new EventHandlerPipeline();
        var @event = new OrderPlacedEvent(Guid.NewGuid());
        var invoked = false;

        // Act
        await pipeline.ExecuteAsync(
            @event,
            () =>
            {
                invoked = true;
                return Task.CompletedTask;
            }
        );

        // Assert
        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithMiddleware_MiddlewareWrapsHandler()
    {
        // Arrange
        var log = new List<string>();
        var pipeline = new EventHandlerPipeline();
        pipeline.Use(
            (evt, next) =>
            {
                log.Add("before");
                var t = next();
                log.Add("after");
                return t;
            }
        );

        var @event = new OrderPlacedEvent(Guid.NewGuid());

        // Act
        await pipeline.ExecuteAsync(
            @event,
            () =>
            {
                log.Add("handler");
                return Task.CompletedTask;
            }
        );

        // Assert
        log.Should().ContainInOrder("before", "handler", "after");
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleMiddlewares_ExecutesInOrder()
    {
        // Arrange
        var log = new List<string>();
        var pipeline = new EventHandlerPipeline();
        pipeline
            .Use(
                async (evt, next) =>
                {
                    log.Add("m1-start");
                    await next();
                    log.Add("m1-end");
                }
            )
            .Use(
                async (evt, next) =>
                {
                    log.Add("m2-start");
                    await next();
                    log.Add("m2-end");
                }
            );

        var @event = new OrderPlacedEvent(Guid.NewGuid());

        // Act
        await pipeline.ExecuteAsync(
            @event,
            () =>
            {
                log.Add("handler");
                return Task.CompletedTask;
            }
        );

        // Assert
        log.Should().ContainInOrder("m1-start", "m2-start", "handler", "m2-end", "m1-end");
    }

    [Fact]
    public void Count_ReflectsNumberOfRegisteredMiddlewares()
    {
        var pipeline = new EventHandlerPipeline();
        pipeline.Count.Should().Be(0);

        pipeline.Use((_, next) => next());
        pipeline.Count.Should().Be(1);

        pipeline.Use((_, next) => next());
        pipeline.Count.Should().Be(2);
    }
}

public sealed class DomainEventRouterTests
{
    private static (DomainEventRouter router, TrackingOrderHandler handler) BuildRouter()
    {
        var handler = new TrackingOrderHandler();
        var services = new ServiceCollection();
        services.AddSingleton<IDomainEventHandler<OrderPlacedEvent>>(handler);
        var sp = services.BuildServiceProvider();

        var pipeline = new EventHandlerPipeline();
        var logger = NullLogger<DomainEventRouter>.Instance;
        var router = new DomainEventRouter(sp, pipeline, logger);
        return (router, handler);
    }

    [Fact]
    public async Task RouteAsync_WithRegisteredHandler_DeliversEvent()
    {
        // Arrange
        var (router, handler) = BuildRouter();
        var @event = new OrderPlacedEvent(Guid.NewGuid());

        // Act
        await router.RouteAsync(@event);

        // Assert
        handler.Received.Should().ContainSingle().Which.Should().Be(@event);
    }

    [Fact]
    public async Task RouteAsync_WithNoHandlers_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();
        var router = new DomainEventRouter(sp, new EventHandlerPipeline(), NullLogger<DomainEventRouter>.Instance);
        var @event = new OrderPlacedEvent(Guid.NewGuid());

        // Act
        var act = () => router.RouteAsync(@event);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RouteAsync_WithMultipleHandlers_DeliversToAll()
    {
        // Arrange
        var first = new TrackingOrderHandler();
        var second = new SecondOrderHandler();
        var services = new ServiceCollection();
        services.AddSingleton<IDomainEventHandler<OrderPlacedEvent>>(first);
        services.AddSingleton<IDomainEventHandler<OrderPlacedEvent>>(second);
        var sp = services.BuildServiceProvider();

        var router = new DomainEventRouter(sp, new EventHandlerPipeline(), NullLogger<DomainEventRouter>.Instance);
        var @event = new OrderPlacedEvent(Guid.NewGuid());

        // Act
        await router.RouteAsync(@event);

        // Assert
        first.Received.Should().ContainSingle();
        second.Received.Should().ContainSingle();
    }

    [Fact]
    public async Task RouteAsync_WithPipeline_MiddlewareIsAppliedPerHandler()
    {
        // Arrange
        int middlewareInvokeCount = 0;
        var pipeline = new EventHandlerPipeline();
        pipeline.Use(
            async (evt, next) =>
            {
                middlewareInvokeCount++;
                await next();
            }
        );

        var first = new TrackingOrderHandler();
        var second = new SecondOrderHandler();
        var services = new ServiceCollection();
        services.AddSingleton<IDomainEventHandler<OrderPlacedEvent>>(first);
        services.AddSingleton<IDomainEventHandler<OrderPlacedEvent>>(second);
        var sp = services.BuildServiceProvider();

        var router = new DomainEventRouter(sp, pipeline, NullLogger<DomainEventRouter>.Instance);
        var @event = new OrderPlacedEvent(Guid.NewGuid());

        // Act
        await router.RouteAsync(@event);

        // Assert – middleware runs once per handler (2 handlers)
        middlewareInvokeCount.Should().Be(2);
    }
}

public sealed class DomainEventDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_IteratesAllEvents()
    {
        // Arrange
        var handler = new TrackingOrderHandler();
        var services = new ServiceCollection();
        services.AddSingleton<IDomainEventHandler<OrderPlacedEvent>>(handler);
        var sp = services.BuildServiceProvider();

        var router = new DomainEventRouter(sp, new EventHandlerPipeline(), NullLogger<DomainEventRouter>.Instance);
        var dispatcher = new DomainEventDispatcher(router, NullLogger<DomainEventDispatcher>.Instance);

        var events = new IDomainEvent[]
        {
            new OrderPlacedEvent(Guid.NewGuid()),
            new OrderPlacedEvent(Guid.NewGuid()),
            new OrderPlacedEvent(Guid.NewGuid()),
        };

        // Act
        await dispatcher.DispatchAsync(events);

        // Assert
        handler.Received.Should().HaveCount(3);
    }

    [Fact]
    public async Task DispatchAsync_WithCancelledToken_StopsDispatch()
    {
        // Arrange
        var handler = new TrackingOrderHandler();
        var services = new ServiceCollection();
        services.AddSingleton<IDomainEventHandler<OrderPlacedEvent>>(handler);
        var sp = services.BuildServiceProvider();

        var router = new DomainEventRouter(sp, new EventHandlerPipeline(), NullLogger<DomainEventRouter>.Instance);
        var dispatcher = new DomainEventDispatcher(router, NullLogger<DomainEventDispatcher>.Instance);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var events = new IDomainEvent[] { new OrderPlacedEvent(Guid.NewGuid()) };

        // Act
        var act = () => dispatcher.DispatchAsync(events, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}

public sealed class CrossBoundaryEventBridgeTests
{
    [Fact]
    public void TryConvert_WithRegisteredConverter_ReturnsTrueAndContract()
    {
        // Arrange
        var bridge = new CrossBoundaryEventBridge();
        bridge.Register<OrderPlacedEvent>(e => new OrderPlacedContract(e.OrderId));

        var @event = new OrderPlacedEvent(Guid.NewGuid());

        // Act
        var result = bridge.TryConvert(@event, out var contract);

        // Assert
        result.Should().BeTrue();
        contract.Should().BeOfType<OrderPlacedContract>().Which.OrderId.Should().Be(@event.OrderId);
    }

    [Fact]
    public void TryConvert_WithoutRegisteredConverter_ReturnsFalse()
    {
        // Arrange
        var bridge = new CrossBoundaryEventBridge();
        var @event = new OrderPlacedEvent(Guid.NewGuid());

        // Act
        var result = bridge.TryConvert(@event, out var contract);

        // Assert
        result.Should().BeFalse();
        contract.Should().BeNull();
    }

    [Fact]
    public void Convert_WithoutRegisteredConverter_ThrowsInvalidOperationException()
    {
        // Arrange
        var bridge = new CrossBoundaryEventBridge();
        var @event = new OrderPlacedEvent(Guid.NewGuid());

        // Act
        var act = () => bridge.Convert(@event);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*OrderPlacedEvent*");
    }

    [Fact]
    public void HasConverter_ReflectsRegistrationState()
    {
        // Arrange
        var bridge = new CrossBoundaryEventBridge();
        bridge.HasConverter<OrderPlacedEvent>().Should().BeFalse();

        // Act
        bridge.Register<OrderPlacedEvent>(e => new OrderPlacedContract(e.OrderId));

        // Assert
        bridge.HasConverter<OrderPlacedEvent>().Should().BeTrue();
    }
}

public sealed class DomainEventRoutingDiTests
{
    private static IServiceCollection AddTestLogging(IServiceCollection services)
    {
        services.AddSingleton(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(global::Microsoft.Extensions.Logging.ILogger<>), typeof(NullLogger<>));
        return services;
    }

    [Fact]
    public void AddDomainEventRouting_RegistersExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        AddTestLogging(services);

        // Act
        services.AddDomainEventRouting();
        var sp = services.BuildServiceProvider();

        // Assert
        sp.GetService<EventHandlerPipeline>().Should().NotBeNull();
        sp.GetService<IDomainEventDispatcher>().Should().BeOfType<DomainEventDispatcher>();
        sp.GetService<CrossBoundaryEventBridge>().Should().NotBeNull();
    }

    [Fact]
    public void AddDomainEventHandler_RegistersHandlerForType()
    {
        // Arrange
        var services = new ServiceCollection();
        AddTestLogging(services);
        services.AddDomainEventRouting();

        // Act
        services.AddDomainEventHandler<OrderPlacedEvent, TrackingOrderHandler>();
        var sp = services.BuildServiceProvider();

        using var scope = sp.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IDomainEventHandler<OrderPlacedEvent>>();

        // Assert
        handlers.Should().ContainSingle().Which.Should().BeOfType<TrackingOrderHandler>();
    }
}
