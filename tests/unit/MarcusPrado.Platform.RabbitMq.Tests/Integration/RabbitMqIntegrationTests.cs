using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Testcontainers.RabbitMq;

namespace MarcusPrado.Platform.RabbitMq.Tests.Integration;

/// <summary>
/// Integration tests for RabbitMQ publish/consume flows.
/// Each test calls <see cref="DockerAvailabilityCheck.SkipIfDockerNotAvailable"/>
/// so the suite is skipped gracefully when Docker is absent.
/// </summary>
public sealed class RabbitMqIntegrationTests : IAsyncLifetime
{
    private RabbitMqContainer? _container;
    private string _connectionString = string.Empty;

    public async Task InitializeAsync()
    {
        DockerAvailabilityCheck.SkipIfDockerNotAvailable();

        _container = new RabbitMqBuilder().Build();
        await _container.StartAsync();
        _connectionString = _container.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.StopAsync();
            await _container.DisposeAsync();
        }
    }

    // ─── helpers ───────────────────────────────────────────────────────────

    private RabbitMqOptions BuildOptions(string? exchange = null) => new()
    {
        ConnectionString = _connectionString,
        Exchange = exchange ?? $"ex-{Guid.NewGuid():N}",
        ExchangeType = "topic",
    };

    private static string UniqueQueueName() => $"q-{Guid.NewGuid():N}";

    private static string UniqueRoutingKey() => $"rk-{Guid.NewGuid():N}";

    // ─── test 5: basic publish / consume ──────────────────────────────────

    [Fact]
    public async Task PublishAndConsume_BasicRoundTrip()
    {
        DockerAvailabilityCheck.SkipIfDockerNotAvailable();

        var opts = BuildOptions();
        var serializer = new JsonMessageSerializer();
        await using var producer = await RabbitProducer.CreateAsync(opts, serializer);

        var routingKey = UniqueRoutingKey();
        var queueName = UniqueQueueName();

        // setup receiver side via raw client
        var factory = new ConnectionFactory { Uri = new Uri(_connectionString) };
        await using var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(opts.Exchange, opts.ExchangeType, durable: true, autoDelete: false);
        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);
        await channel.QueueBindAsync(queueName, opts.Exchange, routingKey);

        await producer.PublishAsync(routingKey, new OrderEvent("ORD-001"));

        var received = new TaskCompletionSource<string>();
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, ea) =>
        {
            received.TrySetResult(Encoding.UTF8.GetString(ea.Body.Span));
            return Task.CompletedTask;
        };
        await channel.BasicConsumeAsync(queueName, autoAck: true, consumer);

        var body = await received.Task.WaitAsync(TimeSpan.FromSeconds(15));
        body.Should().Contain("ORD-001");
    }

    // ─── test 6: DLQ routing — nacked message sent to DLQ exchange ─────────

    [Fact]
    public async Task NackedMessage_CanBeRoutedToDlq()
    {
        DockerAvailabilityCheck.SkipIfDockerNotAvailable();

        var factory = new ConnectionFactory { Uri = new Uri(_connectionString) };
        await using var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        var dlqExchange = $"dlq-{Guid.NewGuid():N}";
        var mainExchange = $"main-{Guid.NewGuid():N}";
        var dlqQueue = $"dlq-q-{Guid.NewGuid():N}";
        var mainQueue = $"main-q-{Guid.NewGuid():N}";
        var routingKey = UniqueRoutingKey();

        // declare DLQ
        await channel.ExchangeDeclareAsync(dlqExchange, "direct", durable: true, autoDelete: false);
        await channel.QueueDeclareAsync(dlqQueue, durable: true, exclusive: false, autoDelete: false);
        await channel.QueueBindAsync(dlqQueue, dlqExchange, routingKey);

        // declare main queue with DLQ settings
        var args = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = dlqExchange,
            ["x-dead-letter-routing-key"] = routingKey,
        };
        await channel.ExchangeDeclareAsync(mainExchange, "direct", durable: true, autoDelete: false);
        await channel.QueueDeclareAsync(mainQueue, durable: true, exclusive: false, autoDelete: false, arguments: args);
        await channel.QueueBindAsync(mainQueue, mainExchange, routingKey);

        // publish a message
        var body = Encoding.UTF8.GetBytes("{\"payload\":\"test-dlq\"}");
        await channel.BasicPublishAsync(mainExchange, routingKey, body: body);

        // consume from main queue and nack (no requeue)
        var mainConsumer = new AsyncEventingBasicConsumer(channel);
        var nackedTcs = new TaskCompletionSource<bool>();
        mainConsumer.ReceivedAsync += async (_, ea) =>
        {
            await channel.BasicNackAsync(ea.DeliveryTag, false, false);
            nackedTcs.TrySetResult(true);
        };
        await channel.BasicConsumeAsync(mainQueue, autoAck: false, mainConsumer);
        await nackedTcs.Task.WaitAsync(TimeSpan.FromSeconds(15));

        // verify DLQ received the message
        var dlqResult = await channel.BasicGetAsync(dlqQueue, autoAck: true);
        dlqResult.Should().NotBeNull();
        Encoding.UTF8.GetString(dlqResult!.Body.Span).Should().Contain("test-dlq");
    }

    // ─── test 7: publisher confirms — channel created with publisher confirmations enabled ────

    [Fact]
    public async Task PublisherConfirms_ChannelWithConfirmationsEnabled_PublishSucceeds()
    {
        DockerAvailabilityCheck.SkipIfDockerNotAvailable();

        var factory = new ConnectionFactory { Uri = new Uri(_connectionString) };
        await using var connection = await factory.CreateConnectionAsync();

        // In RabbitMQ.Client 7.x publisher confirmations are configured via CreateChannelOptions
        var channelOpts = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);
        var channel = await connection.CreateChannelAsync(channelOpts);

        var exchange = $"confirm-ex-{Guid.NewGuid():N}";
        await channel.ExchangeDeclareAsync(exchange, "direct", durable: false, autoDelete: true);

        var body = Encoding.UTF8.GetBytes("confirm-payload");

        var act = async () => await channel.BasicPublishAsync(exchange, "rk", body: body);
        await act.Should().NotThrowAsync();
    }

    // ─── test 8: message TTL — message expires and is gone ────────────────

    [Fact]
    public async Task MessageTtl_ExpiredMessage_IsNotDelivered()
    {
        DockerAvailabilityCheck.SkipIfDockerNotAvailable();

        var factory = new ConnectionFactory { Uri = new Uri(_connectionString) };
        await using var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        var exchange = $"ttl-ex-{Guid.NewGuid():N}";
        var queue = $"ttl-q-{Guid.NewGuid():N}";
        const string rk = "rk-ttl";

        await channel.ExchangeDeclareAsync(exchange, "direct", durable: false, autoDelete: true);
        var args = new Dictionary<string, object?> { ["x-message-ttl"] = 100 };
        await channel.QueueDeclareAsync(queue, durable: false, exclusive: false, autoDelete: true, arguments: args);
        await channel.QueueBindAsync(queue, exchange, rk);

        var props = new BasicProperties { Expiration = "100" };
        var body = Encoding.UTF8.GetBytes("will-expire");
        await channel.BasicPublishAsync(exchange, rk, mandatory: false, basicProperties: props, body: body);

        // Wait for TTL to pass
        await Task.Delay(300);

        var msg = await channel.BasicGetAsync(queue, autoAck: true);
        msg.Should().BeNull("message should have expired");
    }

    // ─── test 9: multiple consumers on same queue get different messages ───

    [Fact]
    public async Task MultipleConsumers_SameQueue_EachMessageDeliveredOnce()
    {
        DockerAvailabilityCheck.SkipIfDockerNotAvailable();

        var factory = new ConnectionFactory { Uri = new Uri(_connectionString) };
        await using var connection = await factory.CreateConnectionAsync();

        var exchange = $"mc-ex-{Guid.NewGuid():N}";
        var queue = $"mc-q-{Guid.NewGuid():N}";
        const string rk = "rk-mc";

        var ch1 = await connection.CreateChannelAsync();
        await ch1.ExchangeDeclareAsync(exchange, "direct", durable: false, autoDelete: true);
        await ch1.QueueDeclareAsync(queue, durable: false, exclusive: false, autoDelete: false);
        await ch1.QueueBindAsync(queue, exchange, rk);

        var ch2 = await connection.CreateChannelAsync();

        // produce 4 messages
        for (var i = 0; i < 4; i++)
        {
            await ch1.BasicPublishAsync(exchange, rk, body: Encoding.UTF8.GetBytes($"msg-{i}"));
        }

        var received1 = new List<string>();
        var received2 = new List<string>();

        var consumer1 = new AsyncEventingBasicConsumer(ch1);
        consumer1.ReceivedAsync += async (_, ea) =>
        {
            lock (received1) received1.Add(Encoding.UTF8.GetString(ea.Body.Span));
            await ch1.BasicAckAsync(ea.DeliveryTag, false);
        };
        await ch1.BasicConsumeAsync(queue, autoAck: false, consumer1);

        var consumer2 = new AsyncEventingBasicConsumer(ch2);
        consumer2.ReceivedAsync += async (_, ea) =>
        {
            lock (received2) received2.Add(Encoding.UTF8.GetString(ea.Body.Span));
            await ch2.BasicAckAsync(ea.DeliveryTag, false);
        };
        await ch2.BasicConsumeAsync(queue, autoAck: false, consumer2);

        var deadline = DateTimeOffset.UtcNow.AddSeconds(15);
        while ((received1.Count + received2.Count) < 4 && DateTimeOffset.UtcNow < deadline)
        {
            await Task.Delay(100);
        }

        var totalReceived = received1.Count + received2.Count;
        totalReceived.Should().Be(4, "all messages should be consumed exactly once across both consumers");
    }

    // ─── test 10: RabbitProducer.CreateAsync + PublishAsync does not throw ─

    [Fact]
    public async Task RabbitProducer_CreateAndPublish_Succeeds()
    {
        DockerAvailabilityCheck.SkipIfDockerNotAvailable();

        var opts = BuildOptions();
        var serializer = new JsonMessageSerializer();

        var act = async () =>
        {
            await using var producer = await RabbitProducer.CreateAsync(opts, serializer);
            await producer.PublishAsync(UniqueRoutingKey(), new OrderEvent("ORD-SMOKE"));
        };

        await act.Should().NotThrowAsync();
    }

    private sealed record OrderEvent(string OrderId);
}
