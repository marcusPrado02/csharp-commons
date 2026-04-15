using System.Text.Json;

namespace MarcusPrado.Platform.ApprovalTestKit;

/// <summary>
/// Snapshots a domain event object by serialising it to JSON with
/// <see cref="System.Text.Json"/> and applying the configured scrubbers.
/// </summary>
public static class DomainEventVerifier
{
    private static readonly JsonSerializerOptions _defaultOptions = new()
    {
        WriteIndented = true,
    };

    /// <summary>
    /// Serialises <paramref name="domainEvent"/> to an indented JSON string, applies all
    /// scrubbers from <paramref name="settings"/>, and returns the scrubbed string.
    /// </summary>
    /// <typeparam name="T">The type of the domain event.</typeparam>
    /// <param name="domainEvent">The domain event object to snapshot.</param>
    /// <param name="settings">
    /// The <see cref="PlatformVerifySettings"/> whose scrubbers to apply.
    /// Pass <see langword="null"/> to use <see cref="PlatformVerifySettings.CreateDefault"/>.
    /// </param>
    /// <returns>The scrubbed JSON string.</returns>
    public static string Snapshot<T>(
        T domainEvent,
        PlatformVerifySettings? settings = null)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var effectiveSettings = settings ?? PlatformVerifySettings.CreateDefault();
        var json = JsonSerializer.Serialize(domainEvent, _defaultOptions);
        return effectiveSettings.Apply(json);
    }
}
