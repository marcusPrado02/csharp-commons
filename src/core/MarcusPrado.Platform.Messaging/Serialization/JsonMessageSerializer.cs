using System.Text.Json;

namespace MarcusPrado.Platform.Messaging.Serialization;

/// <summary><see cref="IMessageSerializer"/> backed by <see cref="System.Text.Json"/>.</summary>
public sealed class JsonMessageSerializer : IMessageSerializer
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    /// <inheritdoc/>
    public string Serialize<T>(T message)
        where T : class
    {
        return JsonSerializer.Serialize(message, _options);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(string data)
        where T : class
    {
        return JsonSerializer.Deserialize<T>(data, _options);
    }
}
