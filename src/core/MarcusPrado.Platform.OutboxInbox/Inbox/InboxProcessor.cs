using MarcusPrado.Platform.OutboxInbox.Idempotency;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

#pragma warning disable CA1873

namespace MarcusPrado.Platform.OutboxInbox.Inbox;

/// <summary>
/// Background service that picks up pending inbox messages, deduplicates them,
/// and dispatches to the appropriate <see cref="IInboxMessageHandler"/>.
/// </summary>
public sealed class InboxProcessor : BackgroundService
{
    private readonly IInboxStore _store;
    private readonly IIdempotencyStore _idempotencyStore;
    private readonly IEnumerable<IInboxMessageHandler> _handlers;
    private readonly InboxProcessorOptions _options;
    private readonly ILogger<InboxProcessor> _logger;

    /// <summary>Initialises a new instance of <see cref="InboxProcessor"/>.</summary>
    public InboxProcessor(
        IInboxStore store,
        IIdempotencyStore idempotencyStore,
        IEnumerable<IInboxMessageHandler> handlers,
        IOptions<InboxProcessorOptions> options,
        ILogger<InboxProcessor> logger
    )
    {
        _store = store;
        _idempotencyStore = idempotencyStore;
        _handlers = handlers;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("InboxProcessor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken).ConfigureAwait(false);
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                _logger.LogError(ex, "InboxProcessor encountered an unhandled error");
            }

            await Task.Delay(_options.PollingInterval, stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        var messages = await _store.GetPendingAsync(_options.BatchSize, ct).ConfigureAwait(false);

        foreach (var msg in messages)
        {
            await ProcessMessageAsync(msg, ct).ConfigureAwait(false);
        }
    }

    private async Task ProcessMessageAsync(InboxMessage msg, CancellationToken ct)
    {
        var key = IdempotencyKey.FromMessageId(msg.MessageId);

        // Deduplication check
        if (await _idempotencyStore.ExistsAsync(key, ct).ConfigureAwait(false))
        {
            _logger.LogDebug("Inbox message {MessageId} is a duplicate, skipping", msg.MessageId);
            await _store.MarkDuplicateAsync(msg.Id, ct).ConfigureAwait(false);
            return;
        }

        var handler = _handlers.FirstOrDefault(h => h.EventType.Equals(msg.EventType, StringComparison.Ordinal));

        if (handler is null)
        {
            _logger.LogWarning("No handler registered for event type {EventType}", msg.EventType);
            await _store.MarkFailedAsync(msg.Id, $"No handler for {msg.EventType}", ct).ConfigureAwait(false);
            return;
        }

        try
        {
            await handler.HandleAsync(msg.Payload, ct).ConfigureAwait(false);

            await _store.MarkProcessedAsync(msg.Id, ct).ConfigureAwait(false);
            await _idempotencyStore
                .SetAsync(
                    new IdempotencyRecord
                    {
                        Key = key.Value,
                        ExpiresAt = DateTimeOffset.UtcNow.Add(_options.IdempotencyTtl),
                    },
                    ct
                )
                .ConfigureAwait(false);

            _logger.LogDebug("Inbox message {MessageId} processed by {Handler}", msg.MessageId, handler.GetType().Name);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            _logger.LogWarning(ex, "Failed to process inbox message {MessageId}", msg.MessageId);

            if (msg.RetryCount >= _options.MaxRetries)
            {
                await _store.MarkFailedAsync(msg.Id, ex.Message, ct).ConfigureAwait(false);
            }
            else
            {
                await _store.IncrementRetryAsync(msg.Id, DateTimeOffset.UtcNow, ct).ConfigureAwait(false);
            }
        }
    }
}
