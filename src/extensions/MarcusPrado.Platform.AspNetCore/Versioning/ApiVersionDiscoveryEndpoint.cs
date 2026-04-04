using MarcusPrado.Platform.AspNetCore.Endpoints;

namespace MarcusPrado.Platform.AspNetCore.Versioning;

/// <summary>
/// Options that declare which API versions are supported and which are deprecated.
/// </summary>
public sealed class VersionManifestOptions
{
    /// <summary>All supported API version strings.</summary>
    public string[] SupportedVersions { get; set; } = ["1.0"];

    /// <summary>Subset of <see cref="SupportedVersions"/> that are deprecated.</summary>
    public string[] DeprecatedVersions { get; set; } = [];
}

/// <summary>
/// Read-model returned by the <c>GET /api-versions</c> endpoint.
/// </summary>
/// <param name="Versions">All supported version strings.</param>
/// <param name="Deprecated">Deprecated version strings.</param>
public sealed record VersionManifest(string[] Versions, string[] Deprecated);

/// <summary>
/// Minimal-API endpoint that exposes <c>GET /api-versions</c> returning a
/// <see cref="VersionManifest"/> built from <see cref="VersionManifestOptions"/>.
/// </summary>
public sealed class ApiVersionDiscoveryEndpoint : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api-versions", (VersionManifestOptions opts) =>
            Results.Ok(new VersionManifest(opts.SupportedVersions, opts.DeprecatedVersions)))
            .WithName("GetApiVersions")
            .WithTags("Versioning")
            .Produces<VersionManifest>();
    }
}

/// <summary>
/// Extension methods for registering and mapping <see cref="ApiVersionDiscoveryEndpoint"/>.
/// </summary>
public static class ApiVersionDiscoveryExtensions
{
    /// <summary>
    /// Registers <see cref="VersionManifestOptions"/> in DI with an optional configuration callback.
    /// </summary>
    public static IServiceCollection AddApiVersionDiscovery(
        this IServiceCollection services,
        Action<VersionManifestOptions>? configure = null)
    {
        var opts = new VersionManifestOptions();
        configure?.Invoke(opts);
        services.AddSingleton(opts);
        return services;
    }
}
