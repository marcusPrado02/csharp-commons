namespace MarcusPrado.Platform.Messaging.Abstractions;

/// <summary>
/// Marker interface for typed message consumers.
/// Concrete implementations also implement <c>IHostedService</c>.
/// </summary>
public interface IMessageConsumer
{
    /// <summary>Gets the topic this consumer subscribes to.</summary>
    string Topic { get; }
}
