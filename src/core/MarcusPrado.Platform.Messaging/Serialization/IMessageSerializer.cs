namespace MarcusPrado.Platform.Messaging.Serialization;

/// <summary>Serialises and deserialises message payloads.</summary>
public interface IMessageSerializer
{
    /// <summary>Serialises <paramref name="message"/> to a string.</summary>
    string Serialize<T>(T message)
        where T : class;

    /// <summary>Deserialises <paramref name="data"/> to <typeparamref name="T"/>.</summary>
    T? Deserialize<T>(string data)
        where T : class;
}
