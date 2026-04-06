using FluentAssertions;
using MarcusPrado.Platform.ChaosKit;
using MarcusPrado.Platform.ChaosKit.Faults;
using MarcusPrado.Platform.ChaosKit.Harness;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MarcusPrado.Platform.ChaosKit.Tests;

// ── ChaosConfig ───────────────────────────────────────────────────────────────

public sealed class ChaosConfigTests
{
    [Fact]
    public void DefaultInjectionRate_IsZero()
    {
        var cfg = new ChaosConfig();
        cfg.InjectionRate.Should().Be(0.0);
    }

    [Fact]
    public void Properties_AreSettable()
    {
        var ex = new InvalidOperationException("boom");
        var cfg = new ChaosConfig
        {
            InjectionRate = 0.5,
            LatencyDelay = TimeSpan.FromMilliseconds(100),
            FaultException = ex,
        };

        cfg.InjectionRate.Should().Be(0.5);
        cfg.LatencyDelay.Should().Be(TimeSpan.FromMilliseconds(100));
        cfg.FaultException.Should().BeSameAs(ex);
    }
}

// ── LatencyFault ──────────────────────────────────────────────────────────────

public sealed class LatencyFaultTests
{
    [Fact]
    public async Task InjectAsync_RateZero_DoesNotDelay()
    {
        var cfg = new ChaosConfig { InjectionRate = 0.0, LatencyDelay = TimeSpan.FromSeconds(60) };
        var fault = new LatencyFault(cfg);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        await fault.InjectAsync();
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    public async Task InjectAsync_NoLatencyConfigured_DoesNotDelay()
    {
        var cfg = new ChaosConfig { InjectionRate = 1.0, LatencyDelay = null };
        var fault = new LatencyFault(cfg);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        await fault.InjectAsync();
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(200);
    }

    [Fact]
    public async Task InjectAsync_RateOne_DelaysConfiguredAmount()
    {
        var cfg = new ChaosConfig { InjectionRate = 1.0, LatencyDelay = TimeSpan.FromMilliseconds(50) };
        var fault = new LatencyFault(cfg);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        await fault.InjectAsync();
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(40);
    }

    [Fact]
    public void Constructor_NullConfig_Throws()
    {
        var act = () => new LatencyFault(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}

// ── ErrorFault ────────────────────────────────────────────────────────────────

public sealed class ErrorFaultTests
{
    [Fact]
    public void Inject_RateZero_DoesNotThrow()
    {
        var cfg = new ChaosConfig
        {
            InjectionRate = 0.0,
            FaultException = new InvalidOperationException("boom"),
        };
        var fault = new ErrorFault(cfg);

        var act = () => fault.Inject();
        act.Should().NotThrow();
    }

    [Fact]
    public void Inject_NoException_DoesNotThrow()
    {
        var cfg = new ChaosConfig { InjectionRate = 1.0, FaultException = null };
        var fault = new ErrorFault(cfg);

        var act = () => fault.Inject();
        act.Should().NotThrow();
    }

    [Fact]
    public void Inject_RateOne_ThrowsConfiguredException()
    {
        var exception = new InvalidOperationException("chaos!");
        var cfg = new ChaosConfig { InjectionRate = 1.0, FaultException = exception };
        var fault = new ErrorFault(cfg);

        var act = () => fault.Inject();
        act.Should().Throw<InvalidOperationException>().WithMessage("chaos!");
    }

    [Fact]
    public void Constructor_NullConfig_Throws()
    {
        var act = () => new ErrorFault(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}

// ── PacketLossFault ───────────────────────────────────────────────────────────

public sealed class PacketLossFaultTests
{
    [Fact]
    public async Task InjectAsync_RateZero_ExecutesAction()
    {
        var cfg = new ChaosConfig { InjectionRate = 0.0 };
        var fault = new PacketLossFault(cfg);
        var executed = false;

        await fault.InjectAsync(() => { executed = true; return Task.CompletedTask; }, _ => { });

        executed.Should().BeTrue();
    }

    [Fact]
    public async Task InjectAsync_RateOne_DropsPacket()
    {
        var cfg = new ChaosConfig { InjectionRate = 1.0 };
        var fault = new PacketLossFault(cfg);
        var executed = false;
        var dropped = false;

        await fault.InjectAsync(
            () => { executed = true; return Task.CompletedTask; },
            wasDropped => dropped = wasDropped);

        executed.Should().BeFalse();
        dropped.Should().BeTrue();
    }

    [Fact]
    public async Task InjectAsync_RateZero_ReportsNotDropped()
    {
        var cfg = new ChaosConfig { InjectionRate = 0.0 };
        var fault = new PacketLossFault(cfg);
        bool? dropped = null;

        await fault.InjectAsync(() => Task.CompletedTask, d => dropped = d);

        dropped.Should().BeFalse();
    }

    [Fact]
    public void Constructor_NullConfig_Throws()
    {
        var act = () => new PacketLossFault(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}

// ── ChaosRunner ───────────────────────────────────────────────────────────────

public sealed class ChaosRunnerTests
{
    [Fact]
    public async Task RunWithChaos_NoFaults_ExecutesAction()
    {
        var cfg = new ChaosConfig { InjectionRate = 0.0 };
        var executed = false;

        await ChaosRunner.RunWithChaos(cfg, () => { executed = true; return Task.CompletedTask; });

        executed.Should().BeTrue();
    }

    [Fact]
    public async Task RunWithChaos_ErrorFaultRateOne_ThrowsBeforeAction()
    {
        var cfg = new ChaosConfig
        {
            InjectionRate = 1.0,
            FaultException = new InvalidOperationException("injected"),
        };
        var executed = false;

        var act = async () => await ChaosRunner.RunWithChaos(
            cfg,
            () => { executed = true; return Task.CompletedTask; });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("injected");
        executed.Should().BeFalse();
    }

    [Fact]
    public void RunWithChaos_NullConfig_Throws()
    {
        var act = async () => await ChaosRunner.RunWithChaos(null!, () => Task.CompletedTask);
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void RunWithChaos_NullAction_Throws()
    {
        var cfg = new ChaosConfig();
        var act = async () => await ChaosRunner.RunWithChaos(cfg, null!);
        act.Should().ThrowAsync<ArgumentNullException>();
    }
}

// ── ChaosExtensions ───────────────────────────────────────────────────────────

public sealed class ChaosExtensionsTests
{
    [Fact]
    public void AddPlatformChaos_RegistersConfig()
    {
        var services = new ServiceCollection();
        services.AddPlatformChaos();

        var sp = services.BuildServiceProvider();
        var cfg = sp.GetRequiredService<ChaosConfig>();

        cfg.Should().NotBeNull();
        cfg.InjectionRate.Should().Be(0.0);
    }

    [Fact]
    public void AddPlatformChaos_WithConfigure_AppliesOptions()
    {
        var services = new ServiceCollection();
        services.AddPlatformChaos(c => c.InjectionRate = 0.75);

        var sp = services.BuildServiceProvider();
        var cfg = sp.GetRequiredService<ChaosConfig>();

        cfg.InjectionRate.Should().Be(0.75);
    }

    [Fact]
    public void AddPlatformChaos_NullServices_Throws()
    {
        var act = () => ChaosExtensions.AddPlatformChaos(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
