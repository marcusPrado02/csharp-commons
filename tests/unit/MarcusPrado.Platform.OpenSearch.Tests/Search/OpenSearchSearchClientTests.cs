namespace MarcusPrado.Platform.OpenSearch.Tests.Search;

public sealed class OpenSearchOptionsTests
{
    [Fact]
    public void DefaultUrl_PointsToLocalhost()
    {
        var opts = new OpenSearchOptions();
        opts.Url.Should().Be("http://localhost:9200");
    }

    [Fact]
    public void DefaultPageSize_Is10()
    {
        var opts = new OpenSearchOptions();
        opts.DefaultPageSize.Should().Be(10);
    }

    [Fact]
    public void UsernameAndPassword_DefaultToNull()
    {
        var opts = new OpenSearchOptions();
        opts.Username.Should().BeNull();
        opts.Password.Should().BeNull();
    }
}

public sealed class OpenSearchSearchClientTests
{
    [Fact]
    public void Constructor_WithNullClient_ThrowsArgumentNullException()
    {
        Action act = () => new OpenSearchSearchClient(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task SearchAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        var settings = new ConnectionSettings(new Uri("http://localhost:9200"));
        var client = new OpenSearchSearchClient(
            new global::OpenSearch.Client.OpenSearchClient(settings));

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => client.SearchAsync<object>(null!));
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyIndexName_ThrowsArgumentException()
    {
        var settings = new ConnectionSettings(new Uri("http://localhost:9200"));
        var client = new OpenSearchSearchClient(
            new global::OpenSearch.Client.OpenSearchClient(settings));

        await Assert.ThrowsAsync<ArgumentException>(
            () => client.GetByIdAsync<object>("", "id-1"));
    }

    [Fact]
    public async Task DeleteDocumentAsync_WithEmptyId_ThrowsArgumentException()
    {
        var settings = new ConnectionSettings(new Uri("http://localhost:9200"));
        var client = new OpenSearchSearchClient(
            new global::OpenSearch.Client.OpenSearchClient(settings));

        await Assert.ThrowsAsync<ArgumentException>(
            () => client.DeleteDocumentAsync("my-index", ""));
    }
}

public sealed class OpenSearchExtensionsTests
{
    [Fact]
    public void AddPlatformOpenSearch_RegistersISearchClientAndIIndexManager()
    {
        var services = new ServiceCollection();
        services.AddPlatformOpenSearch(o => o.Url = "http://localhost:9200");

        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<ISearchClient>()
            .Should().BeOfType<OpenSearchSearchClient>();
        sp.GetRequiredService<IIndexManager>()
            .Should().BeOfType<OpenSearchSearchClient>();
    }

    [Fact]
    public void AddPlatformOpenSearch_BothInterfacesSameInstance()
    {
        var services = new ServiceCollection();
        services.AddPlatformOpenSearch();

        var sp = services.BuildServiceProvider();

        var searchClient = sp.GetRequiredService<ISearchClient>();
        var indexManager = sp.GetRequiredService<IIndexManager>();

        searchClient.Should().BeSameAs(indexManager);
    }

    [Fact]
    public void AddPlatformOpenSearch_WithoutConfigure_UsesDefaults()
    {
        var services = new ServiceCollection();
        services.AddPlatformOpenSearch();

        var sp = services.BuildServiceProvider();
        var opts = sp.GetRequiredService<OpenSearchOptions>();

        opts.Url.Should().Be("http://localhost:9200");
    }
}
