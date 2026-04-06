using System.Text.RegularExpressions;

namespace MarcusPrado.Platform.ApprovalTestKit;

/// <summary>
/// Configures scrubber delegates used to normalise snapshot output.
/// A scrubber is a <see cref="Func{T, TResult}"/> that accepts a raw serialised
/// string and returns a deterministic replacement string, making test assertions
/// stable across runs regardless of volatile values such as GUIDs or timestamps.
/// </summary>
public sealed class PlatformVerifySettings
{
    private static readonly Regex GuidPattern = new(
        @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
        RegexOptions.Compiled,
        TimeSpan.FromSeconds(1));

    // ISO 8601 DateTimeOffset: 2024-01-15T12:34:56.789+00:00 or 2024-01-15T12:34:56Z
    private static readonly Regex DateTimeOffsetPattern = new(
        @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})",
        RegexOptions.Compiled,
        TimeSpan.FromSeconds(1));

    private readonly List<Func<string, string>> _scrubbers = [];

    /// <summary>
    /// Initialises a new <see cref="PlatformVerifySettings"/> instance with no scrubbers registered.
    /// </summary>
    public PlatformVerifySettings()
    {
    }

    /// <summary>
    /// Gets the read-only list of scrubber delegates registered on this instance.
    /// </summary>
    public IReadOnlyList<Func<string, string>> Scrubbers => _scrubbers.AsReadOnly();

    /// <summary>
    /// Adds a scrubber that replaces all GUID-formatted strings with <c>«Guid»</c>.
    /// </summary>
    /// <returns>The current instance for chaining.</returns>
    public PlatformVerifySettings AddGuidScrubber()
    {
        _scrubbers.Add(input => GuidPattern.Replace(input, "«Guid»"));
        return this;
    }

    /// <summary>
    /// Adds a scrubber that replaces all ISO 8601 <see cref="DateTimeOffset"/> strings
    /// with <c>«DateTimeOffset»</c>.
    /// </summary>
    /// <returns>The current instance for chaining.</returns>
    public PlatformVerifySettings AddDateTimeOffsetScrubber()
    {
        _scrubbers.Add(input => DateTimeOffsetPattern.Replace(input, "«DateTimeOffset»"));
        return this;
    }

    /// <summary>
    /// Adds a scrubber that replaces correlation IDs — values in GUID format that appear
    /// in HTTP header lines or structured-log lines — with <c>«CorrelationId»</c>.
    /// The scrubber is intentionally applied after the GUID scrubber in a combined pipeline,
    /// so register <see cref="AddCorrelationIdScrubber"/> <em>before</em>
    /// <see cref="AddGuidScrubber"/> when you want distinct tokens per value type.
    /// </summary>
    /// <returns>The current instance for chaining.</returns>
    public PlatformVerifySettings AddCorrelationIdScrubber()
    {
        // Matches a GUID that follows a header/log keyword such as
        // "X-Correlation-Id: ", "correlationId": ", or "CorrelationId = ".
        var correlationPattern = new Regex(
            @"(?i)(?:x-correlation-(?:id|ID)|correlationid|correlation[-_]id)["":\s=]+([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})",
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        _scrubbers.Add(input => correlationPattern.Replace(
            input,
            m => m.Value.Replace(m.Groups[1].Value, "«CorrelationId»", StringComparison.Ordinal)));

        return this;
    }

    /// <summary>
    /// Registers a custom scrubber delegate.
    /// </summary>
    /// <param name="scrubber">The scrubber to add.</param>
    /// <returns>The current instance for chaining.</returns>
    public PlatformVerifySettings AddScrubber(Func<string, string> scrubber)
    {
        ArgumentNullException.ThrowIfNull(scrubber);
        _scrubbers.Add(scrubber);
        return this;
    }

    /// <summary>
    /// Applies all registered scrubbers in order to the supplied <paramref name="input"/> string.
    /// </summary>
    /// <param name="input">The raw serialised string to scrub.</param>
    /// <returns>The scrubbed string.</returns>
    public string Apply(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return _scrubbers.Aggregate(input, (current, scrubber) => scrubber(current));
    }

    /// <summary>
    /// Creates a <see cref="PlatformVerifySettings"/> instance with all three standard scrubbers
    /// already registered: <see cref="AddCorrelationIdScrubber"/>, <see cref="AddGuidScrubber"/>,
    /// and <see cref="AddDateTimeOffsetScrubber"/>.
    /// </summary>
    /// <returns>A pre-configured <see cref="PlatformVerifySettings"/> instance.</returns>
    public static PlatformVerifySettings CreateDefault()
    {
        return new PlatformVerifySettings()
            .AddCorrelationIdScrubber()
            .AddGuidScrubber()
            .AddDateTimeOffsetScrubber();
    }
}
