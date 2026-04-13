using MarcusPrado.Platform.Abstractions.Search;
using MarcusPrado.Platform.OpenSearch.Options;
using MarcusPrado.Platform.OpenSearch.Search;
using Microsoft.Extensions.DependencyInjection;
using OpenSearch.Client;

namespace MarcusPrado.Platform.OpenSearch.Extensions;

/// <summary>Extension methods to register the OpenSearch adapter.</summary>
public static class OpenSearchExtensions
{
    /// <summary>
    /// Registers <see cref="ISearchClient"/> and <see cref="IIndexManager"/> backed by OpenSearch.
    /// Both interfaces resolve to the same <see cref="OpenSearchSearchClient"/> singleton.
    /// </summary>
    public static IServiceCollection AddPlatformOpenSearch(
        this IServiceCollection services,
        Action<OpenSearchOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var opts = new OpenSearchOptions();
        configure?.Invoke(opts);

        services.AddSingleton(opts);

        services.AddSingleton<IOpenSearchClient>(_ =>
        {
            var settings = new ConnectionSettings(new Uri(opts.Url));
            if (!string.IsNullOrWhiteSpace(opts.Username))
                settings = settings.BasicAuthentication(opts.Username, opts.Password);
            return new global::OpenSearch.Client.OpenSearchClient(settings);
        });

        services.AddSingleton<OpenSearchSearchClient>();
        services.AddSingleton<ISearchClient>(sp => sp.GetRequiredService<OpenSearchSearchClient>());
        services.AddSingleton<IIndexManager>(sp => sp.GetRequiredService<OpenSearchSearchClient>());

        return services;
    }
}
