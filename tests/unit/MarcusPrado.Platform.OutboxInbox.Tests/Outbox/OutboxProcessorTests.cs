using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace MarcusPrado.Platform.OutboxInbox.Tests.Outbox;

public sealed class OutboxProcessorTests
{
    private readonly InMemoryOutboxStore _store = new();
    private readonly IOutboxPublisher _publisher = Substitute.For<IOutboxPublisher>();
    private readonly OutboxProcessor _processor;

    public OutboxProcessorTests()
    {
        var opts = Options.Create(new OutboxProcessorOptions
        {
            PollingInterval = TimeSpan.FromMilliseconds(50),
            BatchSize = 10,
            MaxRetries = 2,
        });
        _processor = new OutboxProcessor(_store, _publisher, opts, NullLogger<OutboxProcessor>.Instance);
    }

    [Fact]
    public async Task PublishSucceeds_MarksMessagePublished()
    {
        var msg = new OutboxMessage { ScheduledAt = DateTimeOffset.UtcNow.AddSeconds(-1) };
        await _store.SaveAsync(msg);

        _publisher.PublishAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        await _processor.StartAsync(cts.Token);
        await Task.Delay(200);
        await _processor.StopAsync(CancellationToken.None);

        Assert.Equal(OutboxState.Published, _store.Messages.First().State);
    }

    [Fact]
    public async Task PublishFails_IncrementsRetryCount()
    {
        var msg = new OutboxMessage { ScheduledAt = DateTimeOffset.UtcNow.AddSeconds(-1) };
        await _store.SaveAsync(msg);

        _publisher.PublishAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("bus down")));

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        await _processor.StartAsync(cts.Token);
        await Task.Delay(200);
        await _processor.StopAsync(CancellationToken.None);

        Assert.Equal(1, _store.Messages.First().RetryCount);
    }

    [Fact]
    public async Task PublishFailsMaxRetries_MarksMessageFailed()
    {
        var msg = new OutboxMessage
        {
            ScheduledAt = DateTimeOffset.UtcNow.AddSeconds(-1),
            RetryCount = 2,
        };
        await _store.SaveAsync(msg);

        _publisher.PublishAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("bus down")));

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        await _processor.StartAsync(cts.Token);
        await Task.Delay(200);
        await _processor.StopAsync(CancellationToken.None);

        Assert.Equal(OutboxState.Failed, _store.Messages.First().State);
    }
}
