using MarcusPrado.Platform.Messaging.Abstractions;
using MarcusPrado.Platform.Messaging.Serialization;
using MarcusPrado.Platform.RabbitMq.Options;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.RabbitMq.Extensions;

/// <summary>Extension methods to register RabbitMQ platform services.</summary>
public static class RabbitMqExtensions
{
    /// <summary>Registers the <see cref="IMessageSerializer"/> for RabbitMQ.</summary>
    public static IServiceCollection AddPlatformRabbitMq(
        this IServiceCollection services,
        Action<RabbitMqOptions>? configure = null)
    {
        var opts = new RabbitMqOptions();
        configure?.Invoke(opts);

        services.AddSingleton(opts);
        services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();

        return services;
    }
}
