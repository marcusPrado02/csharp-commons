using MarcusPrado.Platform.Abstractions.Primitives;

namespace MarcusPrado.Platform.AspNetCore.Internal;

/// <summary>
/// <see cref="IClock"/> implementation that delegates to <see cref="DateTimeOffset.UtcNow"/>.
/// Registered as <c>Singleton</c>; replace with a fake in tests.
/// </summary>
internal sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
