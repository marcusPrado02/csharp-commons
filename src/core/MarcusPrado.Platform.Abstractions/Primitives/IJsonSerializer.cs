namespace MarcusPrado.Platform.Abstractions.Primitives;

/// <summary>
/// Abstraction over JSON serialization, decoupling callers from a specific library.
/// </summary>
public interface IJsonSerializer
{
    /// <summary>Serializes <paramref name="value"/> to its JSON string representation.</summary>
    string Serialize<T>(T value);

    /// <summary>
    /// Deserializes the JSON string to an instance of <typeparamref name="T"/>.
    /// Returns <c>null</c> when <paramref name="json"/> is empty or represents JSON null.
    /// </summary>
    T? Deserialize<T>(string json);
}
