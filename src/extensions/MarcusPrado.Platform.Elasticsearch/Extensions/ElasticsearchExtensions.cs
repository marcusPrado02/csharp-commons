using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using MarcusPrado.Platform.Abstractions.Search;
using MarcusPrado.Platform.Elasticsearch.Options;
using MarcusPrado.Platform.Elasticsearch.Search;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Elasticsearch.Extensions;

/// <summary>Extension methods to register Elasticsearch platform services.</summary>
public static class ElasticsearchExtensions
{
    /// <summary>
    /// Registers <see cref="ISearchClient"/> and <see cref="IIndexManager"/>
    /// backed by Elasticsearch.
    /// </summary>
    public static IServiceCollection AddPlatformElasticsearch(
        this IServiceCollection services,
        Action<ElasticsearchOptions>? configure = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        var opts = new ElasticsearchOptions();
        configure?.Invoke(opts);

        services.AddSingleton(opts);
        services.AddSingleton(_ => BuildClient(opts));
        services.AddSingleton<ElasticsearchSearchClient>();
        services.AddSingleton<ISearchClient>(sp => sp.GetRequiredService<ElasticsearchSearchClient>());
        services.AddSingleton<IIndexManager>(sp => sp.GetRequiredService<ElasticsearchSearchClient>());

        return services;
    }

    private static ElasticsearchClient BuildClient(ElasticsearchOptions opts)
    {
        ElasticsearchClientSettings settings;

        if (!string.IsNullOrWhiteSpace(opts.CloudId))
        {
            settings = new ElasticsearchClientSettings(
                opts.CloudId,
                new BasicAuthentication(opts.Username ?? string.Empty, opts.Password ?? string.Empty)
            );
        }
        else
        {
            var uri = new Uri(opts.Url);
            settings = new ElasticsearchClientSettings(uri);

            if (!string.IsNullOrEmpty(opts.Username))
            {
                settings = settings.Authentication(
                    new BasicAuthentication(opts.Username, opts.Password ?? string.Empty)
                );
            }
        }

        return new ElasticsearchClient(settings);
    }
}
