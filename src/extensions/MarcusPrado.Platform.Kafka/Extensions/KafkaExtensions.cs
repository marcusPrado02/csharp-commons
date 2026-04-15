using MarcusPrado.Platform.Kafka.Options;
using MarcusPrado.Platform.Kafka.Producer;
using MarcusPrado.Platform.Messaging.Abstractions;
using MarcusPrado.Platform.Messaging.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Kafka.Extensions;

/// <summary>Extension methods to register Kafka platform services.</summary>
public static class KafkaExtensions
{
    /// <summary>Registers the <see cref="KafkaProducer"/> and <see cref="IMessageSerializer"/>.</summary>
    public static IServiceCollection AddPlatformKafka(
        this IServiceCollection services,
        Action<KafkaOptions>? configure = null
    )
    {
        var opts = new KafkaOptions();
        configure?.Invoke(opts);

        services.AddSingleton(opts);
        services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
        services.AddSingleton<IMessagePublisher, KafkaProducer>();

        return services;
    }
}
