using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace MarcusPrado.Platform.HotChocolate.Tests;

public sealed class GraphQlErrorTests
{
    [Fact]
    public void Constructor_SetsMessageAndCode()
    {
        var err = new GraphQlError("Test error", "ERR_TEST");
        err.Message.Should().Be("Test error");
        err.Code.Should().Be("ERR_TEST");
    }

    [Fact]
    public void Constructor_EmptyMessage_ThrowsArgumentException()
    {
        Action act = () => new GraphQlError(string.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithExtensions_SetsExtensions()
    {
        var ext = new Dictionary<string, object?> { ["trace"] = "abc" };
        var err = new GraphQlError("msg", extensions: ext);
        err.Extensions.Should().ContainKey("trace");
    }
}

public sealed class PlatformErrorFilterBridgeTests
{
    [Fact]
    public void OnError_NoException_ReturnsOriginalError()
    {
        var platformFilter = Substitute.For<IPlatformErrorFilter>();
        var bridge = new PlatformErrorFilterBridge(platformFilter);

        var error = ErrorBuilder.New().SetMessage("plain error").Build();
        var result = bridge.OnError(error);

        result.Should().BeSameAs(error);
        platformFilter.DidNotReceiveWithAnyArgs().OnError(default!, default!);
    }

    [Fact]
    public void OnError_WithException_DelegatesToPlatformFilter()
    {
        var platformFilter = Substitute.For<IPlatformErrorFilter>();
        platformFilter.OnError(Arg.Any<IGraphQlError>(), Arg.Any<Exception>())
            .Returns(new GraphQlError("Mapped error", "MAPPED"));

        var bridge = new PlatformErrorFilterBridge(platformFilter);

        var error = ErrorBuilder.New()
            .SetMessage("original")
            .SetException(new InvalidOperationException("boom"))
            .Build();

        var result = bridge.OnError(error);

        result.Message.Should().Be("Mapped error");
        result.Code.Should().Be("MAPPED");
    }
}

public sealed class HttpContextResolverContextTests
{
    [Fact]
    public void IsAuthenticated_NoContext_ReturnsFalse()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);

        var ctx = new HttpContextResolverContext(accessor);
        ctx.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void CorrelationId_ReadsFromHeader()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Correlation-Id"] = "corr-123";

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(httpContext);

        var ctx = new HttpContextResolverContext(accessor);
        ctx.CorrelationId.Should().Be("corr-123");
    }

    [Fact]
    public void TenantId_ReadsFromHeader_WhenNoClaimPresent()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tenant-Id"] = "tenant-abc";

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(httpContext);

        var ctx = new HttpContextResolverContext(accessor);
        ctx.TenantId.Should().Be("tenant-abc");
    }

    [Fact]
    public void AddPlatformGraphQL_RegistersIPlatformResolverContext()
    {
        var services = new ServiceCollection();
        services.AddPlatformGraphQL();
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IPlatformResolverContext>()
            .Should().BeOfType<HttpContextResolverContext>();
    }
}
