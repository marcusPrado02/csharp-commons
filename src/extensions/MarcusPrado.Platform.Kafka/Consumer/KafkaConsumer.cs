using Confluent.Kafka;
using MarcusPrado.Platform.Kafka.Options;
using MarcusPrado.Platform.Messaging.Abstractions;
using MarcusPrado.Platform.Messaging.Envelope;
using MarcusPrado.Platform.Messaging.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MarcusPrado.Platform.Kafka.Consumer;

/// <summary>
/// Abstract base for Kafka consumers that process messages of type
/// <typeparamref name="TMessage"/> as a hosted service.
/// </summary>
/// <typeparam name="TMessage">The message payload type.</typeparam>
public abstract partial class KafkaConsumer<TMessage> :
    BackgroundService,
    IMessageConsumer
    where TMessage : class
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IMessageSerializer _serializer;
    private readonly ILogger _logger;

    /// <inheritdoc/>
    public abstract string Topic { get; }

    /// <summary>Initialises the consumer.</summary>
    protected KafkaConsumer(
        KafkaOptions options,
        IMessageSerializer serializer,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        _serializer = serializer;
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = options.BootstrapServers,
            GroupId = options.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var fullTopic = Topic;
        _consumer.Subscribe(fullTopic);
        LogSubscribed(_logger, fullTopic);

        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(stoppingToken);
                if (result is null)
                {
                    continue;
                }

                var envelope = _serializer.Deserialize<MessageEnvelope<TMessage>>(result.Message.Value);
                if (envelope is null)
                {
                    continue;
                }

                await HandleAsync(envelope, stoppingToken);
                _consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException ex)
            {
                LogConsumeError(_logger, ex, fullTopic);
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }
            catch (KafkaException ex)
            {
                LogConsumeError(_logger, ex, fullTopic);
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }
        }

        _consumer.Close();
    }

    /// <summary>Override to handle the deserialized message envelope.</summary>
    protected abstract Task HandleAsync(
        MessageEnvelope<TMessage> envelope,
        CancellationToken ct);

    /// <inheritdoc/>
    public override void Dispose()
    {
        _consumer.Dispose();
        GC.SuppressFinalize(this);
        base.Dispose();
    }

    [LoggerMessage(LogLevel.Information, "Kafka consumer subscribed to {Topic}")]
    private static partial void LogSubscribed(ILogger logger, string topic);

    [LoggerMessage(LogLevel.Error, "Error processing Kafka message from {Topic}")]
    private static partial void LogConsumeError(ILogger logger, Exception ex, string topic);
}
