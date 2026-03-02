using MarcusPrado.Platform.AspNetCore.Endpoints;
using MarcusPrado.Platform.AspNetCore.Filters;
using MarcusPrado.Platform.Contracts.Http;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace MarcusPrado.Platform.AspNetCore.Tests;

public sealed class EndpointConventionsTests
{
    // ── MapPlatformEndpoints ──────────────────────────────────────────────────

    [Fact]
    public void MapPlatformEndpoints_NoEndpointsInAssembly_ReturnsBuilder()
    {
        var routeBuilder = Substitute.For<IEndpointRouteBuilder>();
        routeBuilder.ServiceProvider.Returns(Substitute.For<IServiceProvider>());

        // The test assembly has no IEndpoint implementations
        var result = routeBuilder.MapPlatformEndpoints(typeof(EndpointConventionsTests).Assembly);

        result.Should().BeSameAs(routeBuilder);
    }

    // ── ApiEnvelopeFilter ─────────────────────────────────────────────────────

    [Fact]
    public async Task ApiEnvelopeFilter_WrapsPrimitive_InEnvelope()
    {
        var filter = new ApiEnvelopeFilter();
        var ctx    = BuildContext();

        var result = await filter.InvokeAsync(ctx, _ => ValueTask.FromResult<object?>(42));

        result.Should().BeOfType<ApiEnvelope<object>>();
        var env = (ApiEnvelope<object>)result!;
        env.Success.Should().BeTrue();
        env.Data.Should().Be(42);
    }

    [Fact]
    public async Task ApiEnvelopeFilter_AlreadyEnvelope_PassesThrough()
    {
        var filter  = new ApiEnvelopeFilter();
        var ctx     = BuildContext();
        var existing = new ApiEnvelope<string> { Success = true, Data = "hi" };

        var result = await filter.InvokeAsync(ctx, _ => ValueTask.FromResult<object?>(existing));

        result.Should().BeSameAs(existing);
    }

    [Fact]
    public async Task ApiEnvelopeFilter_NullResult_PassesThrough()
    {
        var filter = new ApiEnvelopeFilter();
        var ctx    = BuildContext();

        var result = await filter.InvokeAsync(ctx, _ => ValueTask.FromResult<object?>(null));

        result.Should().BeNull();
    }

    // ── ApiEnvelope helpers ───────────────────────────────────────────────────

    [Fact]
    public void ApiEnvelope_Ok_SetsSuccessAndData()
    {
        var env = ApiEnvelope.Ok("payload");

        env.Success.Should().BeTrue();
        env.Data.Should().Be("payload");
        env.Timestamp.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ApiEnvelope_Fail_SetsErrorMessage()
    {
        var env = ApiEnvelope.Fail("not found");

        env.Success.Should().BeFalse();
        env.ErrorMessage.Should().Be("not found");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static EndpointFilterInvocationContext BuildContext()
    {
        var ctx = Substitute.For<EndpointFilterInvocationContext>();
        ctx.HttpContext.Returns(new DefaultHttpContext());
        ctx.Arguments.Returns([]);
        return ctx;
    }
}
