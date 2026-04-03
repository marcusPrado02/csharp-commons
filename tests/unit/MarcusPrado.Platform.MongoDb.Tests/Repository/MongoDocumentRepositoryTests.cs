using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.MongoDb.Tests.Repository;

public sealed record Product(string Name, decimal Price);

public sealed class DocumentStoreOptionsTests
{
    [Fact]
    public void Options_StoresConnectionStringAndDatabase()
    {
        var opts = new DocumentStoreOptions("mongodb://localhost", "platform-db", "MyApp");

        opts.ConnectionString.Should().Be("mongodb://localhost");
        opts.DatabaseName.Should().Be("platform-db");
        opts.AppName.Should().Be("MyApp");
    }

    [Fact]
    public void Options_AppNameIsOptional()
    {
        var opts = new DocumentStoreOptions("mongodb://localhost", "db");
        opts.AppName.Should().BeNull();
    }
}

public sealed class MongoDocumentRepositoryTests
{
    [Fact]
    public void Constructor_NullDatabase_ThrowsArgumentNullException()
    {
        Action act = () => new MongoDocumentRepository<Product>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithDatabase_Succeeds()
    {
        var db  = Substitute.For<IMongoDatabase>();
        var col = Substitute.For<IMongoCollection<DocumentEnvelope<Product>>>();
        db.GetCollection<DocumentEnvelope<Product>>(Arg.Any<string>()).Returns(col);

        var act = () => new MongoDocumentRepository<Product>(db);
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithExplicitCollectionName_UsesIt()
    {
        var db  = Substitute.For<IMongoDatabase>();
        var col = Substitute.For<IMongoCollection<DocumentEnvelope<Product>>>();
        db.GetCollection<DocumentEnvelope<Product>>(Arg.Any<string>()).Returns(col);

        _ = new MongoDocumentRepository<Product>(db, "products");

        db.Received(1).GetCollection<DocumentEnvelope<Product>>("products");
    }
}

public sealed class MongoDbExtensionsTests
{
    [Fact]
    public void AddPlatformMongoDb_RegistersIMongoClientAndDatabase()
    {
        var services = new ServiceCollection();
        var opts = new DocumentStoreOptions("mongodb://localhost:27017", "test-db");
        services.AddPlatformMongoDb(opts);
        services.AddDocumentRepository<Product>();

        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IMongoClient>().Should().NotBeNull();
        sp.GetRequiredService<IDocumentRepository<Product>>()
            .Should().BeOfType<MongoDocumentRepository<Product>>();
    }
}
