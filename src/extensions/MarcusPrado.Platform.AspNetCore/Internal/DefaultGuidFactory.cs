using MarcusPrado.Platform.Abstractions.Primitives;

namespace MarcusPrado.Platform.AspNetCore.Internal;

/// <summary>
/// <see cref="IGuidFactory"/> implementation backed by <see cref="Guid.NewGuid"/>.
/// Registered as <c>Singleton</c>; replace with a sequential-ID factory in production or a fixed-Guid fake in tests.
/// </summary>
internal sealed class DefaultGuidFactory : IGuidFactory
{
    /// <inheritdoc />
    public Guid NewGuid() => Guid.NewGuid();
}
