using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

#pragma warning disable CA1873

namespace MarcusPrado.Platform.OutboxInbox.Outbox;

/// <summary>
/// Background service that polls the outbox table and publishes pending messages.
/// </summary>
public sealed class OutboxProcessor : BackgroundService
{
    private readonly IOutboxStore _store;
    private readonly IOutboxPublisher _publisher;
    private readonly OutboxProcessorOptions _options;
    private readonly ILogger<OutboxProcessor> _logger;

    /// <summary>Initialises a new instance of <see cref="OutboxProcessor"/>.</summary>
    public OutboxProcessor(
        IOutboxStore store,
        IOutboxPublisher publisher,
        IOptions<OutboxProcessorOptions> options,
        ILogger<OutboxProcessor> logger)
    {
        _store = store;
        _publisher = publisher;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessor started with interval {Interval}", _options.PollingInterval);

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
                _logger.LogError(ex, "OutboxProcessor encountered an unhandled error");
            }

            await Task.Delay(_options.PollingInterval, stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        var messages = await _store.GetPendingAsync(_options.BatchSize, ct).ConfigureAwait(false);

        if (messages.Count == 0)
        {
            return;
        }

        _logger.LogDebug("OutboxProcessor processing {Count} messages", messages.Count);

        foreach (var msg in messages)
        {
            await ProcessMessageAsync(msg, ct).ConfigureAwait(false);
        }
    }

    private async Task ProcessMessageAsync(OutboxMessage msg, CancellationToken ct)
    {
        try
        {
            await _publisher.PublishAsync(msg, ct).ConfigureAwait(false);
            await _store.MarkPublishedAsync(msg.Id, ct).ConfigureAwait(false);
            _logger.LogDebug("Outbox message {Id} published successfully", msg.Id);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            _logger.LogWarning(ex, "Failed to publish outbox message {Id} (attempt {Attempt})", msg.Id, msg.RetryCount + 1);

            if (msg.RetryCount >= _options.MaxRetries)
            {
                await _store.MarkFailedAsync(msg.Id, ex.Message, ct).ConfigureAwait(false);
            }
            else
            {
                var delay = TimeSpan.FromSeconds(
                    Math.Pow(2, msg.RetryCount) * _options.RetryBaseDelay.TotalSeconds);
                var nextAttempt = DateTimeOffset.UtcNow.Add(delay);
                await _store.IncrementRetryAsync(msg.Id, nextAttempt, ct).ConfigureAwait(false);
            }
        }
    }
}
