namespace MarcusPrado.Platform.TestKit.Tests.Fakes;

public sealed class FakeUserContextTests
{
    [Fact]
    public void Anonymous_IsNotAuthenticated()
    {
        var ctx = FakeUserContext.Anonymous();
        Assert.Null(ctx.UserId);
        Assert.False(ctx.IsAuthenticated);
        Assert.Empty(ctx.Permissions);
    }

    [Fact]
    public void Authenticated_SetsUserId()
    {
        var ctx = FakeUserContext.Authenticated("user42");
        Assert.Equal("user42", ctx.UserId);
        Assert.True(ctx.IsAuthenticated);
    }

    [Fact]
    public void Authenticated_SetsPermissions()
    {
        var ctx = FakeUserContext.Authenticated("u", permissions: ["read", "write"]);
        Assert.Contains("read", ctx.Permissions);
        Assert.Contains("write", ctx.Permissions);
    }
}
