using FluentAssertions;
using MarcusPrado.Platform.Quartz;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Quartz;
using Quartz.Spi;
using Xunit;

namespace MarcusPrado.Platform.Quartz.Tests;

public sealed class PlatformJobFactoryTests
{
    [Fact]
    public void NewJob_ResolvesJobFromServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddTransient<FakeQuartzJob>();
        var sp = services.BuildServiceProvider();

        var factory = new PlatformJobFactory(sp);
        var bundle = CreateBundle(typeof(FakeQuartzJob));

        var job = factory.NewJob(bundle, Substitute.For<IScheduler>());

        job.Should().BeOfType<FakeQuartzJob>();
    }

    [Fact]
    public void NewJob_ActivatesJobWhenNotRegistered()
    {
        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();

        var factory = new PlatformJobFactory(sp);
        var bundle = CreateBundle(typeof(FakeQuartzJob));

        var job = factory.NewJob(bundle, Substitute.For<IScheduler>());

        job.Should().BeOfType<FakeQuartzJob>();
    }

    [Fact]
    public void ReturnJob_DisposesDisposableJob()
    {
        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();
        var factory = new PlatformJobFactory(sp);
        var disposableJob = new DisposableFakeJob();

        factory.ReturnJob(disposableJob);

        disposableJob.Disposed.Should().BeTrue();
    }

    [Fact]
    public void ReturnJob_NonDisposableJob_DoesNotThrow()
    {
        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();
        var factory = new PlatformJobFactory(sp);
        var job = new FakeQuartzJob();

        var act = () => factory.ReturnJob(job);
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_NullServiceProvider_ThrowsArgumentNullException()
    {
        var act = () => new PlatformJobFactory(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("serviceProvider");
    }

    private static TriggerFiredBundle CreateBundle(Type jobType)
    {
        var jobDetail = JobBuilder.Create(jobType).Build();
        var trigger = TriggerBuilder.Create().StartNow().Build();

        return new TriggerFiredBundle(
            jobDetail,
            (IOperableTrigger)trigger,
            null,
            false,
            DateTimeOffset.UtcNow,
            null,
            null,
            null
        );
    }

    private sealed class FakeQuartzJob : global::Quartz.IJob
    {
        public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
    }

    private sealed class DisposableFakeJob : global::Quartz.IJob, IDisposable
    {
        public bool Disposed { get; private set; }

        public Task Execute(IJobExecutionContext context) => Task.CompletedTask;

        public void Dispose() => Disposed = true;
    }
}
