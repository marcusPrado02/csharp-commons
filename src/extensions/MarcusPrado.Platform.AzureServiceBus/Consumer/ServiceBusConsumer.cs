// <copyright file="ServiceBusConsumer.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

namespace MarcusPrado.Platform.AzureServiceBus.Consumer;

/// <summary>Azure Service Bus implementation of <see cref="IServiceBusConsumer"/>.</summary>
public sealed class ServiceBusConsumer : IServiceBusConsumer
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusOptions _options;
    private readonly ILogger<ServiceBusConsumer> _logger;
    private ServiceBusProcessor? _processor;

    /// <summary>Initialises a new instance of <see cref="ServiceBusConsumer"/>.</summary>
    /// <param name="client">The <see cref="ServiceBusClient"/> used to create processors.</param>
    /// <param name="options">The resolved <see cref="ServiceBusOptions"/>.</param>
    /// <param name="logger">The logger.</param>
    public ServiceBusConsumer(
        ServiceBusClient client,
        IOptions<ServiceBusOptions> options,
        ILogger<ServiceBusConsumer> logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task StartAsync(
        string queueOrTopic,
        Func<ServiceBusReceivedMessage, CancellationToken, Task> handler,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(queueOrTopic);
        ArgumentNullException.ThrowIfNull(handler);

        var processorOptions = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = _options.MaxConcurrentCalls,
            MaxAutoLockRenewalDuration = _options.MaxAutoLockRenewalDuration,
        };

        _processor = _client.CreateProcessor(queueOrTopic, processorOptions);

        _processor.ProcessMessageAsync += async args =>
        {
            await handler(args.Message, args.CancellationToken).ConfigureAwait(false);
            await args.CompleteMessageAsync(args.Message, args.CancellationToken).ConfigureAwait(false);
        };

        _processor.ProcessErrorAsync += args =>
        {
            _logger.LogError(
                args.Exception,
                "Error processing Service Bus message from {EntityPath}",
                args.EntityPath);
            return Task.CompletedTask;
        };

        await _processor.StartProcessingAsync(ct).ConfigureAwait(false);

        await Task.Delay(Timeout.Infinite, ct).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

        await _processor.StopProcessingAsync(CancellationToken.None).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_processor is not null)
        {
            await _processor.DisposeAsync().ConfigureAwait(false);
        }
    }
}
