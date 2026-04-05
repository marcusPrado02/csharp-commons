using FluentAssertions;
using MarcusPrado.Platform.BackgroundJobs.Abstractions;
using MarcusPrado.Platform.Quartz;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using Xunit;

namespace MarcusPrado.Platform.Quartz.Tests;

public sealed class QuartzExtensionsTests
{
    [Fact]
    public void AddPlatformQuartz_RegistersIJobScheduler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPlatformQuartz();
        var sp = services.BuildServiceProvider();

        var scheduler = sp.GetService<IJobScheduler>();
        scheduler.Should().NotBeNull();
    }

    [Fact]
    public void AddPlatformQuartz_RegistersPlatformJobFactory()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPlatformQuartz();
        var sp = services.BuildServiceProvider();

        var factory = sp.GetService<IJobFactory>();
        factory.Should().BeOfType<PlatformJobFactory>();
    }

    [Fact]
    public void AddPlatformQuartz_RegistersSchedulerFactory()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPlatformQuartz();
        var sp = services.BuildServiceProvider();

        var factory = sp.GetService<ISchedulerFactory>();
        factory.Should().NotBeNull();
    }

    [Fact]
    public void AddPlatformQuartz_ReturnsServiceCollectionForChaining()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var result = services.AddPlatformQuartz();

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddPlatformQuartz_NullServices_ThrowsArgumentNullException()
    {
        IServiceCollection services = null!;
        var act = () => services.AddPlatformQuartz();

        act.Should().Throw<ArgumentNullException>();
    }
}
