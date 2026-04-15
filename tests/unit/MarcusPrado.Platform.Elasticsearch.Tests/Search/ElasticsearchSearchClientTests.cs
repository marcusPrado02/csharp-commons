using MarcusPrado.Platform.Elasticsearch.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Elasticsearch.Tests.Search;

public sealed class ElasticsearchOptionsTests
{
    [Fact]
    public void DefaultUrl_PointsToLocalhost()
    {
        var opts = new ElasticsearchOptions();
        opts.Url.Should().Be("http://localhost:9200");
    }

    [Fact]
    public void DefaultPageSize_Is10()
    {
        var opts = new ElasticsearchOptions();
        opts.DefaultPageSize.Should().Be(10);
    }

    [Fact]
    public void UsernameAndPassword_DefaultToNull()
    {
        var opts = new ElasticsearchOptions();
        opts.Username.Should().BeNull();
        opts.Password.Should().BeNull();
    }
}

public sealed class SearchQueryTests
{
    [Fact]
    public void SearchQuery_DefaultPagination_Is0And10()
    {
        var q = new SearchQuery("my-index", "hello");
        q.Skip.Should().Be(0);
        q.Take.Should().Be(10);
    }

    [Fact]
    public void SearchQuery_CustomPagination_IsPreserved()
    {
        var q = new SearchQuery("idx", "term", Skip: 20, Take: 5);
        q.Skip.Should().Be(20);
        q.Take.Should().Be(5);
    }

    [Fact]
    public void SearchQuery_FiltersDefaultToNull()
    {
        var q = new SearchQuery("idx", "q");
        q.Filters.Should().BeNull();
    }
}

public sealed class ElasticsearchSearchClientTests
{
    [Fact]
    public void Constructor_WithNullClient_ThrowsArgumentNullException()
    {
        Action act = () => new ElasticsearchSearchClient(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task SearchAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"));
        var client = new ElasticsearchSearchClient(new ElasticsearchClient(settings));

        await Assert.ThrowsAsync<ArgumentNullException>(() => client.SearchAsync<object>(null!));
    }
}

public sealed class ElasticsearchExtensionsTests
{
    [Fact]
    public void AddPlatformElasticsearch_RegistersISearchClientAndIIndexManager()
    {
        var services = new ServiceCollection();
        services.AddPlatformElasticsearch(o => o.Url = "http://localhost:9200");

        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<ISearchClient>().Should().BeOfType<ElasticsearchSearchClient>();
        sp.GetRequiredService<IIndexManager>().Should().BeOfType<ElasticsearchSearchClient>();
    }

    [Fact]
    public void AddPlatformElasticsearch_BothInterfacesSameInstance()
    {
        var services = new ServiceCollection();
        services.AddPlatformElasticsearch();

        var sp = services.BuildServiceProvider();

        var searchClient = sp.GetRequiredService<ISearchClient>();
        var indexManager = sp.GetRequiredService<IIndexManager>();

        searchClient.Should().BeSameAs(indexManager);
    }
}
