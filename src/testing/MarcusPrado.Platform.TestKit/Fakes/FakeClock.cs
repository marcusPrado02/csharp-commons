using MarcusPrado.Platform.Abstractions.Primitives;

namespace MarcusPrado.Platform.TestKit.Fakes;

/// <summary>
/// Deterministic clock implementation for use in unit and integration tests.
/// Time is frozen at construction and only advances when explicitly mutated.
/// </summary>
public sealed class FakeClock : IClock
{
    private DateTimeOffset _now;

    /// <summary>Initialises the fake clock at <see cref="DateTimeOffset.UtcNow"/>.</summary>
    public FakeClock()
        : this(DateTimeOffset.UtcNow)
    {
    }

    /// <summary>Initialises the fake clock at the given <paramref name="startTime"/>.</summary>
    public FakeClock(DateTimeOffset startTime)
    {
        _now = startTime;
    }

    /// <inheritdoc/>
    public DateTimeOffset UtcNow => _now;

    /// <summary>Replaces the current time with <paramref name="newTime"/>.</summary>
    public void SetNow(DateTimeOffset newTime) => _now = newTime;

    /// <summary>Advances the clock by <paramref name="duration"/>.</summary>
    public void Advance(TimeSpan duration) => _now = _now.Add(duration);
}
