using System.Text.Json;
using MarcusPrado.Platform.Abstractions.Primitives;

namespace MarcusPrado.Platform.AspNetCore.Internal;

/// <summary>
/// <see cref="IJsonSerializer"/> implementation backed by <see cref="System.Text.Json"/>.
/// Uses camelCase property naming and handles non-ASCII characters without escaping.
/// </summary>
internal sealed class SystemTextJsonSerializer : IJsonSerializer
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
    };

    /// <inheritdoc />
    public string Serialize<T>(T value)
        => JsonSerializer.Serialize(value, _options);

    /// <inheritdoc />
    public T? Deserialize<T>(string json)
        => JsonSerializer.Deserialize<T>(json, _options);
}
