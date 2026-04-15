using MarcusPrado.Platform.Abstractions.Storage;
using MarcusPrado.Platform.MongoDb.Repository;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace MarcusPrado.Platform.MongoDb.Extensions;

/// <summary>Extension methods to register MongoDB document store services.</summary>
public static class MongoDbExtensions
{
    /// <summary>
    /// Registers <see cref="IMongoClient"/>, <see cref="IMongoDatabase"/>,
    /// and <see cref="IDocumentRepository{T}"/> backed by MongoDB.
    /// </summary>
    public static IServiceCollection AddPlatformMongoDb(this IServiceCollection services, DocumentStoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton(options);
        services.AddSingleton<IMongoClient>(_ =>
        {
            var settings = MongoClientSettings.FromConnectionString(options.ConnectionString);
            if (!string.IsNullOrEmpty(options.AppName))
                settings.ApplicationName = options.AppName;
            return new MongoClient(settings);
        });
        services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(options.DatabaseName));

        return services;
    }

    /// <summary>
    /// Registers <see cref="IDocumentRepository{T}"/> for a specific document type,
    /// storing documents in <paramref name="collectionName"/> (or the type name if omitted).
    /// </summary>
    public static IServiceCollection AddDocumentRepository<T>(
        this IServiceCollection services,
        string? collectionName = null
    )
        where T : class
    {
        services.AddSingleton<IDocumentRepository<T>>(sp => new MongoDocumentRepository<T>(
            sp.GetRequiredService<IMongoDatabase>(),
            collectionName
        ));

        return services;
    }
}
