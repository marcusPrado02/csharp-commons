using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using MarcusPrado.Platform.Hangfire.Attributes;
using MarcusPrado.Platform.Hangfire.Registrar;
using MarcusPrado.Platform.Hangfire.Scheduler;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Reflection;
using Xunit;

namespace MarcusPrado.Platform.Hangfire.Tests;

public sealed class HangfireRecurringJobRegistrarTests
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly HangfireRecurringJobRegistrar _sut;

    public HangfireRecurringJobRegistrarTests()
    {
        _recurringJobManager = Substitute.For<IRecurringJobManager>();
        _sut = new HangfireRecurringJobRegistrar(
            _recurringJobManager,
            NullLogger<HangfireRecurringJobRegistrar>.Instance);
    }

    [Fact]
    public void FindAttributedTypes_FindsDecoratedJob()
    {
        var results = HangfireRecurringJobRegistrar.FindAttributedTypes(
            Assembly.GetExecutingAssembly()).ToList();

        results.Should().ContainSingle(r => r.JobType == typeof(MarkedJob));
    }

    [Fact]
    public void FindAttributedTypes_DoesNotFindUndecorated()
    {
        var results = HangfireRecurringJobRegistrar.FindAttributedTypes(
            Assembly.GetExecutingAssembly()).ToList();

        results.Should().NotContain(r => r.JobType == typeof(UnmarkedJob));
    }

    [Fact]
    public void FindAttributedTypes_ReturnsCorrectCronExpression()
    {
        var results = HangfireRecurringJobRegistrar.FindAttributedTypes(
            Assembly.GetExecutingAssembly()).ToList();

        results.Single(r => r.JobType == typeof(MarkedJob))
               .Attribute.CronExpression
               .Should().Be("*/5 * * * *");
    }

    [Fact]
    public void RegisterFromAssembly_RegistersMarkedJob()
    {
        // RecurringJobManagerExtensions.AddOrUpdate<T> delegates to IRecurringJobManager.AddOrUpdate(id, job, cron, opts)
        _sut.RegisterFromAssembly(Assembly.GetExecutingAssembly());

        _recurringJobManager.Received(1).AddOrUpdate(
            Arg.Any<string>(),
            Arg.Is<Job>(j => j.Type == typeof(MarkedJob)),
            "*/5 * * * *",
            Arg.Any<RecurringJobOptions>());
    }

    [Fact]
    public void RegisterFromAssembly_NullAssembly_ThrowsArgumentNullException()
    {
        var act = () => _sut.RegisterFromAssembly(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("assembly");
    }

    [Fact]
    public void FindAttributedTypes_NullAssemblies_ThrowsArgumentNullException()
    {
        var act = () => HangfireRecurringJobRegistrar.FindAttributedTypes(null!).ToList();

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("assemblies");
    }

    // ── helpers ─────────────────────────────────────────────────────────────────

    [RecurringJob("*/5 * * * *")]
    private sealed class MarkedJob : IHangfireJob
    {
        public Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class UnmarkedJob : IHangfireJob
    {
        public Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
