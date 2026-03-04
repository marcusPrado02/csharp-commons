using MarcusPrado.Platform.MultiTenancy.Context;

namespace MarcusPrado.Platform.MultiTenancy.Tests;

public sealed class TenantResolverTests
{
    [Fact]
    public void HeaderTenantResolver_WithHeader_ReturnsTenantId()
    {
        var sut     = new HeaderTenantResolver();
        var headers = new Dictionary<string, string> { ["x-tenant-id"] = "tenant-1" };
        sut.Resolve(headers).Should().Be("tenant-1");
    }

    [Fact]
    public void HeaderTenantResolver_MissingHeader_ReturnsNull()
    {
        var sut     = new HeaderTenantResolver();
        var headers = new Dictionary<string, string>();
        sut.Resolve(headers).Should().BeNull();
    }

    [Fact]
    public void HeaderTenantResolver_EmptyHeader_ReturnsNull()
    {
        var sut     = new HeaderTenantResolver();
        var headers = new Dictionary<string, string> { ["x-tenant-id"] = "  " };
        sut.Resolve(headers).Should().BeNull();
    }

    [Fact]
    public void JwtTenantResolver_MissingHeader_ReturnsNull()
    {
        var sut     = new JwtTenantResolver();
        var headers = new Dictionary<string, string>();
        sut.Resolve(headers).Should().BeNull();
    }

    [Fact]
    public void JwtTenantResolver_InvalidToken_ReturnsNull()
    {
        var sut     = new JwtTenantResolver();
        var headers = new Dictionary<string, string> { ["authorization"] = "Bearer not-a-jwt" };
        sut.Resolve(headers).Should().BeNull();
    }

    [Fact]
    public void TenantContext_SetTenant_IsResolved()
    {
        var ctx = new TenantContext();
        ctx.IsResolved.Should().BeFalse();
        ctx.SetTenant("tenant-abc");
        ctx.IsResolved.Should().BeTrue();
        ctx.TenantId.Should().Be("tenant-abc");
    }

    [Fact]
    public void TenantContext_SetTenantEmpty_ThrowsArgumentException()
    {
        var ctx = new TenantContext();
        var act = () => ctx.SetTenant("");
        act.Should().Throw<ArgumentException>();
    }
}
