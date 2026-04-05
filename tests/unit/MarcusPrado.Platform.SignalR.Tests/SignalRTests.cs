namespace MarcusPrado.Platform.SignalR.Tests;

public sealed class TestHub : Hub { }

/// <summary>
/// A minimal domain event used by the sink tests.
/// </summary>
internal sealed class OrderPlacedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;

    public string EventType => "order.placed";
}

public sealed class HubRealtimePublisherTests
{
    // ── Test 1: PublishAsync calls Clients.All.SendCoreAsync with correct topic ──

    [Fact]
    public async Task PublishAsync_ShouldCallClientsAllSendCoreAsync_WithCorrectTopic()
    {
        // Arrange
        var mockContext = Substitute.For<IHubContext<TestHub>>();
        var mockClients = Substitute.For<IHubClients>();
        var mockAll = Substitute.For<IClientProxy>();
        mockContext.Clients.Returns(mockClients);
        mockClients.All.Returns(mockAll);

        var publisher = new HubRealtimePublisher<TestHub>(mockContext);
        var payload = new { value = 1 };

        // Act
        await publisher.PublishAsync("test-topic", payload);

        // Assert
        await mockAll.Received(1).SendCoreAsync(
            "test-topic",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    // ── Test 2: PublishToTenantAsync calls Clients.Group("tenant:t1").SendCoreAsync ──

    [Fact]
    public async Task PublishToTenantAsync_ShouldCallClientsGroupSendCoreAsync_WithTenantGroup()
    {
        // Arrange
        var mockContext = Substitute.For<IHubContext<TestHub>>();
        var mockClients = Substitute.For<IHubClients>();
        var mockGroup = Substitute.For<IClientProxy>();
        mockContext.Clients.Returns(mockClients);
        mockClients.Group("tenant:t1").Returns(mockGroup);

        var publisher = new HubRealtimePublisher<TestHub>(mockContext);

        // Act
        await publisher.PublishToTenantAsync("t1", "order-topic", new { id = 42 });

        // Assert
        await mockGroup.Received(1).SendCoreAsync(
            "order-topic",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    // ── Test 3: PublishToUserAsync calls Clients.User("u1").SendCoreAsync ──

    [Fact]
    public async Task PublishToUserAsync_ShouldCallClientsUserSendCoreAsync()
    {
        // Arrange
        var mockContext = Substitute.For<IHubContext<TestHub>>();
        var mockClients = Substitute.For<IHubClients>();
        var mockUser = Substitute.For<IClientProxy>();
        mockContext.Clients.Returns(mockClients);
        mockClients.User("u1").Returns(mockUser);

        var publisher = new HubRealtimePublisher<TestHub>(mockContext);

        // Act
        await publisher.PublishToUserAsync("u1", "user-topic", new { msg = "hello" });

        // Assert
        await mockUser.Received(1).SendCoreAsync(
            "user-topic",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }
}

public sealed class SignalRDomainEventSinkTests
{
    // ── Test 4: HandleAsync calls IRealtimePublisher.PublishAsync with snake_case topic ──

    [Fact]
    public async Task HandleAsync_ShouldCallPublishAsync_WithSnakeCaseTopic()
    {
        // Arrange
        var mockPublisher = Substitute.For<IRealtimePublisher>();
        var sink = new SignalRDomainEventSink(mockPublisher);
        var domainEvent = new OrderPlacedEvent();

        // Act
        await sink.HandleAsync(domainEvent);

        // Assert
        await mockPublisher.Received(1).PublishAsync(
            "order_placed_event",
            Arg.Any<IDomainEvent>(),
            Arg.Any<CancellationToken>());
    }

    // ── Test 5: ToSnakeCase("OrderPlaced") → "order_placed" ──

    [Theory]
    [InlineData("OrderPlaced", "order_placed")]
    [InlineData("UserRegistered", "user_registered")]
    [InlineData("PaymentProcessed", "payment_processed")]
    [InlineData("SingleWord", "single_word")]
    [InlineData("ABC", "a_b_c")]
    public void ToSnakeCase_ShouldConvertPascalCaseToSnakeCase(string input, string expected)
    {
        // Act
        var result = SignalRDomainEventSink.ToSnakeCase(input);

        // Assert
        result.Should().Be(expected);
    }
}

public sealed class SignalRExtensionsTests
{
    // ── Test 6: AddPlatformSignalR<TestHub> → IRealtimePublisher resolves from DI ──

    [Fact]
    public void AddPlatformSignalR_ShouldRegisterIRealtimePublisher_InDI()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddPlatformSignalR<TestHub>();
        var provider = services.BuildServiceProvider();

        // Assert
        var publisher = provider.GetService<IRealtimePublisher>();
        publisher.Should().NotBeNull();
        publisher.Should().BeOfType<HubRealtimePublisher<TestHub>>();
    }
}
