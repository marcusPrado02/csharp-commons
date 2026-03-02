namespace MarcusPrado.Platform.Messaging.Abstractions;

/// <summary>
/// Marker interface for typed message consumers.
/// Concrete implementations also implement <see cref="Microsoft.Extensions.Hosting.IHostedService"/>.
/// </summary>
public interface IMessageConsumer
{
    /// <summary>Gets the topic this consumer subscribes to.</summary>
    string Topic { get; }
}
