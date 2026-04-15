namespace MarcusPrado.Platform.EfCore.Tests.DbContext;

public sealed class TenantFilterTests : IDisposable
{
    // Use a shared in-memory database name so all records appear in the same store.
    private readonly string _dbName = Guid.NewGuid().ToString();
    private readonly FakeTenantContext _tenantContext;
    private readonly TenantTestDbContext _ctx;

    public TenantFilterTests()
    {
        _tenantContext = new FakeTenantContext("tenant-A");
        _ctx = TenantTestDbContext.CreateInMemory(_dbName, _tenantContext);
        _ctx.Database.EnsureCreated();
    }

    [Fact]
    public async Task Query_ReturnsOnlyCurrentTenantEntities_WhenFilterIsActive()
    {
        _ctx.TenantEntities.AddRange(
            new TenantTestEntity { Name = "A1", TenantId = "tenant-A" },
            new TenantTestEntity { Name = "B1", TenantId = "tenant-B" }
        );
        await _ctx.SaveChangesAsync();

        var results = await _ctx.TenantEntities.ToListAsync();

        Assert.All(results, e => Assert.Equal("tenant-A", e.TenantId));
    }

    [Fact]
    public async Task Query_ExcludesCrossTenantEntities_WhenFilterIsActive()
    {
        _ctx.TenantEntities.Add(new TenantTestEntity { Name = "B2", TenantId = "tenant-B" });
        await _ctx.SaveChangesAsync();

        var results = await _ctx.TenantEntities.ToListAsync();

        Assert.DoesNotContain(results, e => e.TenantId == "tenant-B");
    }

    [Fact]
    public async Task Query_ReturnsAllEntities_WhenFilterIsIgnored()
    {
        _ctx.TenantEntities.AddRange(
            new TenantTestEntity { Name = "A3", TenantId = "tenant-A" },
            new TenantTestEntity { Name = "B3", TenantId = "tenant-B" }
        );
        await _ctx.SaveChangesAsync();

        var results = await _ctx.TenantEntities.IgnoreQueryFilters().ToListAsync();

        Assert.True(results.Count >= 2);
        Assert.Contains(results, e => e.TenantId == "tenant-A");
        Assert.Contains(results, e => e.TenantId == "tenant-B");
    }

    public void Dispose() => _ctx.Dispose();
}
