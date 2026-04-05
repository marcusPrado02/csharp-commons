using FluentAssertions;
using MarcusPrado.Platform.BackgroundJobs.Abstractions;
using MarcusPrado.Platform.Hangfire.Extensions;
using MarcusPrado.Platform.Hangfire.Options;
using MarcusPrado.Platform.Hangfire.Registrar;
using MarcusPrado.Platform.Hangfire.Scheduler;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MarcusPrado.Platform.Hangfire.Tests;

public sealed class HangfireExtensionsTests
{
    [Fact]
    public void AddPlatformHangfire_RegistersIJobScheduler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPlatformHangfire();

        var provider = services.BuildServiceProvider();

        provider.GetService<IJobScheduler>().Should().NotBeNull();
    }

    [Fact]
    public void AddPlatformHangfire_IJobSchedulerIsHangfireJobScheduler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPlatformHangfire();

        var provider = services.BuildServiceProvider();
        var scheduler = provider.GetRequiredService<IJobScheduler>();

        scheduler.Should().BeOfType<HangfireJobScheduler>();
    }

    [Fact]
    public void AddPlatformHangfire_RegistersHangfireRecurringJobRegistrar()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPlatformHangfire();

        var provider = services.BuildServiceProvider();

        provider.GetService<HangfireRecurringJobRegistrar>().Should().NotBeNull();
    }

    [Fact]
    public void AddPlatformHangfire_RegistersHangfireOptions()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPlatformHangfire(opts => opts.WorkerCount = 10);

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<HangfireOptions>();

        options.WorkerCount.Should().Be(10);
    }

    [Fact]
    public void AddPlatformHangfire_DefaultOptions_DashboardPathIsHangfire()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPlatformHangfire();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<HangfireOptions>();

        options.DashboardPath.Should().Be("/hangfire");
    }
}
