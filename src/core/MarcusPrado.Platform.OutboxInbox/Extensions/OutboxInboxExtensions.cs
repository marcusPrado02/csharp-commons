using MarcusPrado.Platform.OutboxInbox.Idempotency;
using MarcusPrado.Platform.OutboxInbox.Inbox;
using MarcusPrado.Platform.OutboxInbox.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.OutboxInbox.Extensions;

/// <summary>DI extension methods for the OutboxInbox module.</summary>
public static class OutboxInboxExtensions
{
    /// <summary>
    /// Registers the in-memory outbox/inbox stores, the idempotency store, and both processors.
    /// Use in tests or applications that do not need EF Core persistence.
    /// </summary>
    public static IServiceCollection AddInMemoryOutboxInbox(
        this IServiceCollection services,
        Action<OutboxProcessorOptions>? configureOutbox = null,
        Action<InboxProcessorOptions>? configureInbox = null)
    {
        services.AddSingleton<IOutboxStore, InMemoryOutboxStore>();
        services.AddSingleton<IInboxStore, InMemoryInboxStore>();
        services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();

        if (configureOutbox is not null)
        {
            services.Configure(configureOutbox);
        }
        else
        {
            services.Configure<OutboxProcessorOptions>(_ => { });
        }

        if (configureInbox is not null)
        {
            services.Configure(configureInbox);
        }
        else
        {
            services.Configure<InboxProcessorOptions>(_ => { });
        }

        services.AddHostedService<OutboxProcessor>();
        services.AddHostedService<InboxProcessor>();

        return services;
    }

    /// <summary>Registers a custom <see cref="IOutboxPublisher"/> implementation.</summary>
    public static IServiceCollection AddOutboxPublisher<TPublisher>(
        this IServiceCollection services)
        where TPublisher : class, IOutboxPublisher
    {
        services.AddSingleton<IOutboxPublisher, TPublisher>();
        return services;
    }

    /// <summary>Registers an <see cref="IInboxMessageHandler"/> for a specific event type.</summary>
    public static IServiceCollection AddInboxHandler<THandler>(
        this IServiceCollection services)
        where THandler : class, IInboxMessageHandler
    {
        services.AddSingleton<IInboxMessageHandler, THandler>();
        return services;
    }
}
