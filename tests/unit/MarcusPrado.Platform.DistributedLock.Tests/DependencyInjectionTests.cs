using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.DistributedLock.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddPlatformDistributedLock_RegistersIDistributedLock()
    {
        var multiplexer = Substitute.For<StackExchange.Redis.IConnectionMultiplexer>();
        var db = Substitute.For<IDatabase>();
        multiplexer.GetDatabase(Arg.Any<int>(), Arg.Any<object?>()).Returns(db);

        var services = new ServiceCollection();
        services.AddSingleton(multiplexer);
        services.AddPlatformDistributedLock();

        using var provider = services.BuildServiceProvider();
        var resolved = provider.GetService<IDistributedLock>();

        resolved.Should().NotBeNull();
        resolved.Should().BeOfType<RedisDistributedLock>();
    }
}
