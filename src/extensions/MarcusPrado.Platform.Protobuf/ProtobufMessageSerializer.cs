using MarcusPrado.Platform.Messaging.Serialization;
using ProtoBuf;

namespace MarcusPrado.Platform.Protobuf;

/// <summary>
/// <see cref="IMessageSerializer"/> implementation backed by protobuf-net.
/// Binary protobuf bytes are Base64-encoded to satisfy the <c>string</c>-based
/// serialiser contract.  Types must be annotated with
/// <see cref="ProtoContractAttribute"/> and <see cref="ProtoMemberAttribute"/>.
/// </summary>
public sealed class ProtobufMessageSerializer : IMessageSerializer
{
    /// <inheritdoc />
    public string Serialize<T>(T message)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(message);
        using var ms = new MemoryStream();
        Serializer.Serialize(ms, message);
        return Convert.ToBase64String(ms.ToArray());
    }

    /// <inheritdoc />
    public T? Deserialize<T>(string data)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(data);
        var bytes = Convert.FromBase64String(data);
        using var ms = new MemoryStream(bytes);
        return Serializer.Deserialize<T>(ms);
    }
}
