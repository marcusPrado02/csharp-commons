namespace MarcusPrado.Platform.TestKit.Tests.Fakes;

public sealed class FakeTenantContextTests
{
    [Fact]
    public void For_SetsTenantId()
    {
        var ctx = FakeTenantContext.For("acme");
        Assert.Equal("acme", ctx.TenantId);
    }

    [Fact]
    public void SetTenantId_UpdatesTenantId()
    {
        var ctx = FakeTenantContext.For("old");
        ctx.SetTenantId("new");
        Assert.Equal("new", ctx.TenantId);
    }
}
