namespace MarcusPrado.Platform.AspNetCore.Localization;

/// <summary>
/// Configuration options for the platform localization infrastructure.
/// </summary>
public sealed class PlatformLocalizationOptions
{
    /// <summary>
    /// Gets or sets the default culture to use when the request does not specify one
    /// or when the requested culture is not supported.
    /// Defaults to <c>"en-US"</c>.
    /// </summary>
    public string DefaultCulture { get; set; } = "en-US";

    /// <summary>
    /// Gets or sets the list of culture names that this application supports.
    /// Defaults to <c>["en-US", "pt-BR", "es-ES"]</c>.
    /// </summary>
    public string[] SupportedCultures { get; set; } = ["en-US", "pt-BR", "es-ES"];
}
