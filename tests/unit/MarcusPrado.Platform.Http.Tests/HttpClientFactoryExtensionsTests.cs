using MarcusPrado.Platform.Http.Clients;
using MarcusPrado.Platform.Http.Extensions;
using MarcusPrado.Platform.Http.Handlers;
using MarcusPrado.Platform.Abstractions.Context;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace MarcusPrado.Platform.Http.Tests;

public sealed class HttpClientFactoryExtensionsTests
{
    [Fact]
    public void AddPlatformHttpClient_RegistersHandlers()
    {
        var sp = BuildServiceProvider<SampleHttpClient>();

        // Handler types should be resolvable as transients
        sp.GetService<CorrelationHeaderHandler>().Should().NotBeNull();
        sp.GetService<TenantHeaderHandler>().Should().NotBeNull();
    }

    [Fact]
    public void AddPlatformHttpClient_RegistersTypedClient()
    {
        var sp = BuildServiceProvider<SampleHttpClient>();

        sp.GetService<SampleHttpClient>().Should().NotBeNull();
    }

    [Fact]
    public void HttpClientOptions_Defaults()
    {
        var opts = new HttpClientOptions();

        opts.Timeout.Should().Be(TimeSpan.FromSeconds(30));
        opts.BaseAddress.Should().BeNull();
    }

    [Fact]
    public void CorrelationHeaderHandler_NullCorrelation_Throws()
    {
        var act = () => new CorrelationHeaderHandler(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TenantHeaderHandler_NullTenant_Throws()
    {
        var act = () => new TenantHeaderHandler(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    private static IServiceProvider BuildServiceProvider<TClient>()
        where TClient : TypedHttpClient
    {
        var correlation = Substitute.For<ICorrelationContext>();
        var tenant      = Substitute.For<ITenantContext>();

        return new ServiceCollection()
            .AddLogging()
            .AddSingleton(correlation)
            .AddSingleton(tenant)
            .AddScoped<ICorrelationContext>(_ => correlation)
            .AddScoped<ITenantContext>(_ => tenant)
            .AddPlatformHttpClient<TClient>()
            .BuildServiceProvider();
    }
}

/// <summary>Minimal concrete TypedHttpClient for testing registration.</summary>
internal sealed class SampleHttpClient : TypedHttpClient
{
    public SampleHttpClient(System.Net.Http.HttpClient http, Microsoft.Extensions.Logging.ILogger<SampleHttpClient> logger)
        : base(http, logger) { }
}
