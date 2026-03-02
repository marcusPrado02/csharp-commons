namespace MarcusPrado.Platform.Runtime.Configuration;

/// <summary>
/// Type-safe wrapper for a configuration key, preventing mix-up of keys
/// that share the same underlying string name.
/// </summary>
/// <typeparam name="T">Expected CLR type of the configuration value.</typeparam>
#pragma warning disable S2326 // phantom type — T is intentionally unused in the body
public sealed record ConfigurationKey<T>(string Name);
#pragma warning restore S2326
