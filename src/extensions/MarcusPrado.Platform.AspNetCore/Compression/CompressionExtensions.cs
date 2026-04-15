using Microsoft.AspNetCore.ResponseCompression;

namespace MarcusPrado.Platform.AspNetCore.Extensions;

/// <summary>
/// Extension methods for configuring response compression middleware.
/// </summary>
public static class CompressionExtensions
{
    private static readonly string[] _additionalMimeTypes = ["application/json", "application/x-protobuf"];

    /// <summary>
    /// Configures Brotli (primary) + Gzip (fallback) response compression.
    /// Compressed MIME types: application/json, text/plain, text/html,
    /// text/css, application/javascript, application/x-protobuf.
    /// Both providers use <see cref="System.IO.Compression.CompressionLevel.Fastest"/>.
    /// HTTPS compression is enabled by default.
    /// </summary>
    public static IServiceCollection AddPlatformResponseCompression(
        this IServiceCollection services,
        Action<ResponseCompressionOptions>? configure = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddResponseCompression(opts =>
        {
            opts.EnableForHttps = true;
            opts.Providers.Add<BrotliCompressionProvider>();
            opts.Providers.Add<GzipCompressionProvider>();
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(_additionalMimeTypes);
            configure?.Invoke(opts);
        });

        services.Configure<BrotliCompressionProviderOptions>(o =>
            o.Level = System.IO.Compression.CompressionLevel.Fastest
        );
        services.Configure<GzipCompressionProviderOptions>(o =>
            o.Level = System.IO.Compression.CompressionLevel.Fastest
        );

        return services;
    }
}
