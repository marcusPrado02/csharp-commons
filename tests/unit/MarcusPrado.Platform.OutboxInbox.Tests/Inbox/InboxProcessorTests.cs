using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace MarcusPrado.Platform.OutboxInbox.Tests.Inbox;

public sealed class InboxProcessorTests
{
    private readonly InMemoryInboxStore _store = new();
    private readonly InMemoryIdempotencyStore _idempotencyStore = new();
    private readonly IInboxMessageHandler _handler = Substitute.For<IInboxMessageHandler>();

    private InboxProcessor BuildProcessor()
    {
        _handler.EventType.Returns("OrderCreated");
        var opts = Options.Create(new InboxProcessorOptions
        {
            PollingInterval = TimeSpan.FromMilliseconds(50),
            BatchSize = 10,
            MaxRetries = 2,
        });
        return new InboxProcessor(_store, _idempotencyStore, new[] { _handler }, opts, NullLogger<InboxProcessor>.Instance);
    }

    [Fact]
    public async Task HandlerCalled_MessageMarkedProcessed()
    {
        _handler.HandleAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var msg = new InboxMessage { MessageId = "msg-1", EventType = "OrderCreated" };
        await _store.SaveAsync(msg);

        var processor = BuildProcessor();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        await processor.StartAsync(cts.Token);
        await Task.Delay(200);
        await processor.StopAsync(CancellationToken.None);

        Assert.Equal(InboxState.Processed, _store.Messages.First().State);
    }

    [Fact]
    public async Task DuplicateMessage_MarkedDuplicate()
    {
        _handler.HandleAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Store idempotency record before processing to simulate duplicate
        var key = IdempotencyKey.FromMessageId("dup-msg");
        await _idempotencyStore.SetAsync(new IdempotencyRecord { Key = key.Value });

        var msg = new InboxMessage { MessageId = "dup-msg", EventType = "OrderCreated" };
        await _store.SaveAsync(msg);

        var processor = BuildProcessor();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        await processor.StartAsync(cts.Token);
        await Task.Delay(200);
        await processor.StopAsync(CancellationToken.None);

        Assert.Equal(InboxState.Duplicate, _store.Messages.First().State);
        await _handler.DidNotReceive().HandleAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NoHandler_MessageMarkedFailed()
    {
        var msg = new InboxMessage { MessageId = "msg-unknown", EventType = "UnknownEvent" };
        await _store.SaveAsync(msg);

        var processor = BuildProcessor();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        await processor.StartAsync(cts.Token);
        await Task.Delay(200);
        await processor.StopAsync(CancellationToken.None);

        Assert.Equal(InboxState.Failed, _store.Messages.First().State);
    }
}
