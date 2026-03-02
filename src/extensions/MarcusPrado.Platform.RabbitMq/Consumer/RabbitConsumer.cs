using System.Text;
using MarcusPrado.Platform.Messaging.Abstractions;
using MarcusPrado.Platform.Messaging.Envelope;
using MarcusPrado.Platform.Messaging.Serialization;
using MarcusPrado.Platform.RabbitMq.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace MarcusPrado.Platform.RabbitMq.Consumer;

/// <summary>
/// Abstract base for RabbitMQ consumers that process messages of type
/// <typeparamref name="TMessage"/> as a hosted service.
/// </summary>
/// <typeparam name="TMessage">The message payload type.</typeparam>
public abstract partial class RabbitConsumer<TMessage> :
    BackgroundService,
    IMessageConsumer
    where TMessage : class
{
    private readonly RabbitMqOptions _options;
    private readonly IMessageSerializer _serializer;
    private readonly ILogger _logger;
    private IChannel? _channel;

    /// <inheritdoc/>
    public abstract string Topic { get; }

    /// <summary>Gets the queue name this consumer binds to.</summary>
    public abstract string QueueName { get; }

    /// <summary>Initialises the consumer.</summary>
    protected RabbitConsumer(
        RabbitMqOptions options,
        IMessageSerializer serializer,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
        _serializer = serializer;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { Uri = new Uri(_options.ConnectionString) };
        var connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(
            exchange: _options.Exchange,
            type: _options.ExchangeType,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue: QueueName,
            exchange: _options.Exchange,
            routingKey: Topic,
            cancellationToken: stoppingToken);

        await _channel.BasicQosAsync(0, _options.PrefetchCount, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnReceivedAsync;

        await _channel.BasicConsumeAsync(QueueName, false, consumer, stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
    }

    private async Task OnReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        if (_channel is null)
        {
            return;
        }

        try
        {
            var body = Encoding.UTF8.GetString(ea.Body.Span);
            var envelope = _serializer.Deserialize<MessageEnvelope<TMessage>>(body);
            if (envelope is not null)
            {
                await HandleAsync(envelope, CancellationToken.None);
            }

            await _channel.BasicAckAsync(ea.DeliveryTag, false);
        }
        catch (AlreadyClosedException ex)
        {
            LogMessageError(_logger, ex, QueueName);
        }
        catch (OperationInterruptedException ex)
        {
            LogMessageError(_logger, ex, QueueName);
            if (_channel is not null)
            {
                await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
            }
        }
        catch (System.Text.Json.JsonException ex)
        {
            LogMessageError(_logger, ex, QueueName);
            if (_channel is not null)
            {
                await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
            }
        }
    }

    /// <summary>Override to handle the deserialized message envelope.</summary>
    protected abstract Task HandleAsync(
        MessageEnvelope<TMessage> envelope,
        CancellationToken ct);

    /// <inheritdoc/>
    public override void Dispose()
    {
        _channel?.Dispose();
        GC.SuppressFinalize(this);
        base.Dispose();
    }

    [LoggerMessage(LogLevel.Error, "Error processing RabbitMQ message from {Queue}")]
    private static partial void LogMessageError(ILogger logger, Exception ex, string queue);
}
