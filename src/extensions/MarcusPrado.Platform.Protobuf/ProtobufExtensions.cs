using MarcusPrado.Platform.Messaging.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Protobuf;

/// <summary>Registers protobuf-net as the default <see cref="IMessageSerializer"/>.</summary>
public static class ProtobufExtensions
{
    /// <summary>
    /// Replaces (or adds) the <see cref="IMessageSerializer"/> registration with
    /// <see cref="ProtobufMessageSerializer"/>.
    /// </summary>
    public static IServiceCollection AddProtobufSerializer(this IServiceCollection services)
    {
        services.AddSingleton<IMessageSerializer, ProtobufMessageSerializer>();
        return services;
    }
}
