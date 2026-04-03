using MarcusPrado.Platform.Consul.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Consul.Tests.ServiceDiscovery;

public sealed class ConsulOptionsTests
{
    [Fact]
    public void DefaultAddress_PointsToLocalhost()
    {
        var opts = new ConsulOptions();
        opts.Address.Should().Be("http://localhost:8500");
    }

    [Fact]
    public void DefaultCheckInterval_Is10Seconds()
    {
        var opts = new ConsulOptions();
        opts.DefaultCheckInterval.Should().Be(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void TokenDefaultsToNull()
    {
        var opts = new ConsulOptions();
        opts.Token.Should().BeNull();
    }
}

public sealed class ConsulExtensionsTests
{
    [Fact]
    public void AddPlatformConsul_RegistersIServiceDiscovery()
    {
        var services = new ServiceCollection();
        services.AddPlatformConsul(o => o.Address = "http://localhost:8500");

        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IServiceDiscovery>()
            .Should().BeOfType<ConsulServiceDiscovery>();
    }
}

public sealed class ConsulServiceDiscoveryTests
{
    private static ConsulServiceDiscovery BuildService(
        IConsulClient client, ConsulOptions? opts = null)
    {
        return new ConsulServiceDiscovery(client, opts ?? new ConsulOptions());
    }

    [Fact]
    public async Task ResolveAsync_EmptyResults_ReturnsEmptyList()
    {
        var health = Substitute.For<IHealthEndpoint>();
        var client = Substitute.For<IConsulClient>();
        client.Health.Returns(health);

        health.Service(
                Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new QueryResult<ServiceEntry[]>
            {
                Response = [],
            }));

        var svc = BuildService(client);

        var result = await svc.ResolveAsync("empty-service");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ResolveAsync_MapsPassingHealthCheck()
    {
        var health = Substitute.For<IHealthEndpoint>();
        var client = Substitute.For<IConsulClient>();
        client.Health.Returns(health);

        var entry = new ServiceEntry
        {
            Service = new AgentService
            {
                ID = "svc-1", Service = "my-svc",
                Address = "10.0.0.1", Port = 8080, Tags = ["v1"],
            },
            Checks = [new HealthCheck { Status = HealthStatus.Passing }],
        };

        health.Service(
                Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new QueryResult<ServiceEntry[]>
            {
                Response = [entry],
            }));

        var svc = BuildService(client);
        var result = await svc.ResolveAsync("my-svc");

        result.Should().HaveCount(1);
        result[0].ServiceId.Should().Be("svc-1");
        result[0].Address.Should().Be("10.0.0.1");
        result[0].Port.Should().Be(8080);
        result[0].Health.Should().Be(ServiceHealth.Passing);
    }

    [Fact]
    public async Task DeregisterAsync_CallsAgentDeregister()
    {
        var agent = Substitute.For<IAgentEndpoint>();
        var client = Substitute.For<IConsulClient>();
        client.Agent.Returns(agent);
        agent.ServiceDeregister(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new WriteResult()));

        var svc = BuildService(client);
        await svc.DeregisterAsync("service-123");

        await agent.Received(1).ServiceDeregister("service-123", Arg.Any<CancellationToken>());
    }
}
