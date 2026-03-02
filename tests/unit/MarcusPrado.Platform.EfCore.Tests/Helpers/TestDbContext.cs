using Microsoft.EntityFrameworkCore;

namespace MarcusPrado.Platform.EfCore.Tests.Helpers;

/// <summary>Minimal <see cref="AppDbContextBase"/> for unit tests.</summary>
internal sealed class TestDbContext : AppDbContextBase
{
    /// <summary>Creates a <see cref="TestDbContext"/> with InMemory provider.</summary>
    public static TestDbContext CreateInMemory(string dbName = "test-db")
    {
        var opts = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new TestDbContext(opts);
    }

    private TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }
}
