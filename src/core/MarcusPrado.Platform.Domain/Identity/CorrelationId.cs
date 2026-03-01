namespace MarcusPrado.Platform.Domain.Identity;

/// <summary>
/// Strongly-typed correlation / trace identifier.
/// Backed by a <see cref="string"/> to accommodate W3C TraceContext,
/// UUID, and other external formats.
/// </summary>
public sealed record CorrelationId(string Value) : EntityId<string>(Value)
{
    /// <summary>Creates a new random <see cref="CorrelationId"/> backed by a <see cref="Guid"/>.</summary>
    public static CorrelationId New() => new(Guid.NewGuid().ToString());

    /// <summary>Converts a raw <see cref="string"/> to a <see cref="CorrelationId"/> implicitly.</summary>
    public static implicit operator CorrelationId(string value) => new(value);

    /// <summary>Extracts the underlying <see cref="string"/> from a <see cref="CorrelationId"/>.</summary>
    public static implicit operator string(CorrelationId id) => id.Value;
}
