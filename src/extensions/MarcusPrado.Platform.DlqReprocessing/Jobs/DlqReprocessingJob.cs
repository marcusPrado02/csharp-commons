namespace MarcusPrado.Platform.DlqReprocessing.Jobs;

/// <summary>
/// Background service that periodically inspects the DLQ, emits depth metrics,
/// logs threshold warnings, and optionally reprocesses messages via a provided handler.
/// </summary>
public sealed partial class DlqReprocessingJob : BackgroundService
{
    private readonly IDlqStore _store;
    private readonly IDlqMetrics _metrics;
    private readonly DlqOptions _options;
    private readonly ILogger<DlqReprocessingJob> _logger;
    private readonly Func<DlqMessage, CancellationToken, Task<bool>>? _reprocessHandler;

    /// <summary>
    /// Initialises a new <see cref="DlqReprocessingJob"/>.
    /// </summary>
    /// <param name="store">The DLQ store to poll.</param>
    /// <param name="metrics">Metrics sink for DLQ instruments.</param>
    /// <param name="options">Resolved configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="reprocessHandler">
    /// Optional async delegate invoked for each message.
    /// Return <see langword="true"/> to remove the message from the DLQ after reprocessing.
    /// </param>
    public DlqReprocessingJob(
        IDlqStore store,
        IDlqMetrics metrics,
        IOptions<DlqOptions> options,
        ILogger<DlqReprocessingJob> logger,
        Func<DlqMessage, CancellationToken, Task<bool>>? reprocessHandler = null)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _store = store;
        _metrics = metrics;
        _options = options.Value;
        _logger = logger;
        _reprocessHandler = reprocessHandler;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var topicList = string.Join(", ", _options.Topics);
        Log.JobStarted(_logger, _options.PollingIntervalSeconds, _options.AlertThreshold, topicList);

        var interval = TimeSpan.FromSeconds(_options.PollingIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollAsync(stoppingToken).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex) when (ex is not OperationCanceledException)
#pragma warning restore CA1031
            {
                Log.PollingError(_logger, ex);
            }

            await Task.Delay(interval, stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task PollAsync(CancellationToken ct)
    {
        foreach (var topic in _options.Topics)
        {
            var messages = await _store.GetAsync(topic, ct).ConfigureAwait(false);
            var depth = messages.Count;

            _metrics.RecordDepth(topic, depth);

            if (depth > _options.AlertThreshold)
                Log.DepthAlert(_logger, depth, _options.AlertThreshold, topic);

            if (_reprocessHandler is not null)
            {
                foreach (var message in messages)
                {
                    bool success;
                    try
                    {
                        success = await _reprocessHandler(message, ct).ConfigureAwait(false);
                    }
#pragma warning disable CA1031
                    catch (Exception ex) when (ex is not OperationCanceledException)
#pragma warning restore CA1031
                    {
                        Log.ReprocessHandlerFailed(_logger, ex, message.Id, topic);
                        continue;
                    }

                    if (success)
                    {
                        await _store.RequeueAsync(topic, message.Id, ct).ConfigureAwait(false);
                        _metrics.RecordReprocessed(topic);
                    }
                }
            }
        }
    }

    private static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information,
            Message = "DlqReprocessingJob started. PollingInterval={Interval}s, AlertThreshold={Threshold}, Topics={Topics}")]
        internal static partial void JobStarted(ILogger logger, int interval, int threshold, string topics);

        [LoggerMessage(Level = LogLevel.Error,
            Message = "Unhandled error in DlqReprocessingJob polling cycle.")]
        internal static partial void PollingError(ILogger logger, Exception ex);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "DLQ depth {Depth} exceeds alert threshold {Threshold} for topic '{Topic}'.")]
        internal static partial void DepthAlert(ILogger logger, int depth, int threshold, string topic);

        [LoggerMessage(Level = LogLevel.Error,
            Message = "Reprocess handler failed for message '{Id}' on topic '{Topic}'.")]
        internal static partial void ReprocessHandlerFailed(ILogger logger, Exception ex, string id, string topic);
    }
}
