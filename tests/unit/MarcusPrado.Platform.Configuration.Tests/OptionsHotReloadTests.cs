using FluentAssertions;
using MarcusPrado.Platform.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace MarcusPrado.Platform.Configuration.Tests;

// Options type must be public for NSubstitute to proxy IOptionsMonitor<T>
public sealed class HotReloadOptionsModel
{
    public string Value { get; set; } = string.Empty;
}

public sealed class OptionsHotReloadTests
{
    private static ConfigurationChangeLogger CreateChangeLogger()
        => new(NullLogger<ConfigurationChangeLogger>.Instance);

    [Fact]
    public void CurrentValue_ShouldReturnMonitorCurrentValue()
    {
        var monitor = Substitute.For<IOptionsMonitor<HotReloadOptionsModel>>();
        var options = new HotReloadOptionsModel { Value = "hello" };
        monitor.CurrentValue.Returns(options);
        monitor.OnChange(Arg.Any<Action<HotReloadOptionsModel, string?>>()).Returns(Substitute.For<IDisposable>());

        var hotReload = new OptionsHotReload<HotReloadOptionsModel>(monitor, CreateChangeLogger());

        hotReload.CurrentValue.Should().BeSameAs(options);
    }

    [Fact]
    public void OnChange_ShouldInvokeListenerWhenMonitorChanges()
    {
        var monitor = Substitute.For<IOptionsMonitor<HotReloadOptionsModel>>();
        var options = new HotReloadOptionsModel { Value = "initial" };
        monitor.CurrentValue.Returns(options);

        Action<HotReloadOptionsModel, string?>? capturedMonitorCallback = null;
        monitor.OnChange(Arg.Do<Action<HotReloadOptionsModel, string?>>(cb => capturedMonitorCallback = cb))
               .Returns(Substitute.For<IDisposable>());

        var hotReload = new OptionsHotReload<HotReloadOptionsModel>(monitor, CreateChangeLogger());

        HotReloadOptionsModel? receivedOptions = null;
        hotReload.OnChange(opts => receivedOptions = opts);

        var newOptions = new HotReloadOptionsModel { Value = "updated" };
        capturedMonitorCallback!(newOptions, null);

        receivedOptions.Should().BeSameAs(newOptions);
    }

    [Fact]
    public void OnChange_ShouldReturnDisposable()
    {
        var monitor = Substitute.For<IOptionsMonitor<HotReloadOptionsModel>>();
        monitor.CurrentValue.Returns(new HotReloadOptionsModel());
        monitor.OnChange(Arg.Any<Action<HotReloadOptionsModel, string?>>()).Returns(Substitute.For<IDisposable>());

        var hotReload = new OptionsHotReload<HotReloadOptionsModel>(monitor, CreateChangeLogger());

        var disposable = hotReload.OnChange(_ => { });

        disposable.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullMonitor_ShouldThrowArgumentNullException()
    {
        var act = () => new OptionsHotReload<HotReloadOptionsModel>(null!, CreateChangeLogger());

        act.Should().Throw<ArgumentNullException>().WithParameterName("monitor");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        var monitor = Substitute.For<IOptionsMonitor<HotReloadOptionsModel>>();

        var act = () => new OptionsHotReload<HotReloadOptionsModel>(monitor, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("changeLogger");
    }

    [Fact]
    public void OnChange_WithNullListener_ShouldThrowArgumentNullException()
    {
        var monitor = Substitute.For<IOptionsMonitor<HotReloadOptionsModel>>();
        monitor.CurrentValue.Returns(new HotReloadOptionsModel());
        monitor.OnChange(Arg.Any<Action<HotReloadOptionsModel, string?>>()).Returns(Substitute.For<IDisposable>());

        var hotReload = new OptionsHotReload<HotReloadOptionsModel>(monitor, CreateChangeLogger());

        var act = () => hotReload.OnChange(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
