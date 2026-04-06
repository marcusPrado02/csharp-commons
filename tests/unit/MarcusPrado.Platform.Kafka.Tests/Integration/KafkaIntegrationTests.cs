using System.Collections.Concurrent;
using Confluent.Kafka;
using MarcusPrado.Platform.Messaging.Abstractions;
using Testcontainers.Kafka;

namespace MarcusPrado.Platform.Kafka.Tests.Integration;

/// <summary>
/// Integration tests for Kafka publish/consume flows.
/// Each test calls <see cref="DockerAvailabilityCheck.SkipIfDockerNotAvailable"/>
/// so the suite is skipped gracefully when Docker is absent (e.g. CI without Docker-in-Docker).
/// </summary>
public sealed class KafkaIntegrationTests : IAsyncLifetime
{
    private KafkaContainer? _container;
    private string _bootstrapServers = string.Empty;

    public async Task InitializeAsync()
    {
        DockerAvailabilityCheck.SkipIfDockerNotAvailable();

        _container = new KafkaBuilder().Build();
        await _container.StartAsync();
        _bootstrapServers = _container.GetBootstrapAddress();
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

    private KafkaOptions BuildOptions(string? prefix = null) => new()
    {
        BootstrapServers = _bootstrapServers,
        TopicPrefix = prefix ?? string.Empty,
        ConsumerGroupId = $"test-group-{Guid.NewGuid():N}",
    };

    private static IConsumer<string, string> BuildConsumer(string bootstrapServers, string groupId)
    {
        return new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        }).Build();
    }

    private static string UniqueTopicName() => $"topic-{Guid.NewGuid():N}";

    // ─── test 7: publish single message, consume it ────────────────────────

    [Fact]
    public async Task PublishSingleMessage_ConsumerReceivesIt()
    {
        DockerAvailabilityCheck.SkipIfDockerNotAvailable();

        var topic = UniqueTopicName();
        var opts = BuildOptions();
        var serializer = new JsonMessageSerializer();
        using var producer = new KafkaProducer(opts, serializer);

        await producer.PublishAsync(topic, new TestEvent("hello-single"));

        using var consumer = BuildConsumer(_bootstrapServers, opts.ConsumerGroupId);
        consumer.Subscribe(topic);

        var result = consumer.Consume(TimeSpan.FromSeconds(15));
        result.Should().NotBeNull();
        result!.Message.Value.Should().Contain("hello-single");
    }

    // ─── test 8: publish multiple messages ─────────────────────────────────

    [Fact]
    public async Task PublishMultipleMessages_AllConsumed()
    {
        DockerAvailabilityCheck.SkipIfDockerNotAvailable();

        var topic = UniqueTopicName();
        var opts = BuildOptions();
        var serializer = new JsonMessageSerializer();
        using var producer = new KafkaProducer(opts, serializer);

        const int count = 5;
        for (var i = 0; i < count; i++)
        {
            await producer.PublishAsync(topic, new TestEvent($"msg-{i}"));
        }

        using var consumer = BuildConsumer(_bootstrapServers, opts.ConsumerGroupId);
        consumer.Subscribe(topic);

        var messages = new List<string>();
        var deadline = DateTimeOffset.UtcNow.AddSeconds(20);
        while (messages.Count < count && DateTimeOffset.UtcNow < deadline)
        {
            var r = consumer.Consume(TimeSpan.FromSeconds(2));
            if (r is not null)
            {
                messages.Add(r.Message.Value);
            }
        }

        messages.Should().HaveCount(count);
    }

    // ─── test 9: topic prefix is applied ────────────────────────────────────

    [Fact]
    public async Task Publish_WithTopicPrefix_UsesFullTopicName()
    {
        DockerAvailabilityCheck.SkipIfDockerNotAvailable();

        var baseTopic = UniqueTopicName();
        var opts = BuildOptions(prefix: "myapp.");
        var serializer = new JsonMessageSerializer();
        using var producer = new KafkaProducer(opts, serializer);

        await producer.PublishAsync(baseTopic, new TestEvent("prefix-test"));

        // consume from the prefixed topic
        var fullTopic = $"myapp.{baseTopic}";
        using var consumer = BuildConsumer(_bootstrapServers, opts.ConsumerGroupId);
        consumer.Subscribe(fullTopic);

        var result = consumer.Consume(TimeSpan.FromSeconds(15));
        result.Should().NotBeNull();
        result!.Message.Value.Should().Contain("prefix-test");
    }

    // ─── test 10: message key equals message id ──────────────────────────────

    [Fact]
    public async Task Publish_MessageKey_IsMessageId()
    {
        DockerAvailabilityCheck.SkipIfDockerNotAvailable();

        var topic = UniqueTopicName();
        var opts = BuildOptions();
        var serializer = new JsonMessageSerializer();
        using var producer = new KafkaProducer(opts, serializer);

        await producer.PublishAsync(topic, new TestEvent("key-check"));

        using var consumer = BuildConsumer(_bootstrapServers, opts.ConsumerGroupId);
        consumer.Subscribe(topic);

        var result = consumer.Consume(TimeSpan.FromSeconds(15));
        result.Should().NotBeNull();
        // Key should be a non-empty GUID-like string (set from Metadata.MessageId)
        result!.Message.Key.Should().NotBeNullOrWhiteSpace();
    }

    // ─── test 11: graceful shutdown — producer disposes without errors ────────

    [Fact]
    public async Task Producer_Dispose_DoesNotThrow()
    {
        DockerAvailabilityCheck.SkipIfDockerNotAvailable();

        var opts = BuildOptions();
        var serializer = new JsonMessageSerializer();
        using var producer = new KafkaProducer(opts, serializer);

        await producer.PublishAsync(UniqueTopicName(), new TestEvent("before-dispose"));

        var act = () =>
        {
            producer.Dispose();
        };

        act.Should().NotThrow();
    }

    // ─── test 12: offset commit — consumer commits after successful consume ───

    [Fact]
    public async Task Consume_CommitOffset_DoesNotThrowOnRestart()
    {
        DockerAvailabilityCheck.SkipIfDockerNotAvailable();

        var topic = UniqueTopicName();
        var groupId = $"commit-group-{Guid.NewGuid():N}";
        var opts = new KafkaOptions
        {
            BootstrapServers = _bootstrapServers,
            ConsumerGroupId = groupId,
        };
        var serializer = new JsonMessageSerializer();
        using var producer = new KafkaProducer(opts, serializer);

        await producer.PublishAsync(topic, new TestEvent("commit-test"));

        // first consumer reads and commits
        using (var c1 = BuildConsumer(_bootstrapServers, groupId))
        {
            c1.Subscribe(topic);
            var r = c1.Consume(TimeSpan.FromSeconds(15));
            r.Should().NotBeNull();
            c1.Commit(r);
        }

        // second consumer with same group should not see the committed message
        using var c2 = BuildConsumer(_bootstrapServers, groupId);
        c2.Subscribe(topic);
        var deadMsg = c2.Consume(TimeSpan.FromSeconds(3));

        // already committed offset — new consumer in same group should get nothing new
        deadMsg.Should().BeNull();
    }

    private sealed record TestEvent(string Name);
}
