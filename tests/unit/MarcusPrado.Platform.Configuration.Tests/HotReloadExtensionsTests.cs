using FluentAssertions;
using MarcusPrado.Platform.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace MarcusPrado.Platform.Configuration.Tests;

public sealed class HotReloadExtensionsTests
{
    private sealed class MyOptions
    {
        public string Value { get; set; } = string.Empty;
    }

    [Fact]
    public void AddPlatformOptionsHotReload_ShouldRegisterIOptionsHotReload()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions<MyOptions>();
        services.AddPlatformOptionsHotReload<MyOptions>();

        var provider = services.BuildServiceProvider();

        var hotReload = provider.GetService<IOptionsHotReload<MyOptions>>();
        hotReload.Should().NotBeNull();
    }

    [Fact]
    public void AddPlatformOptionsHotReload_ShouldRegisterConfigurationChangeLogger()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions<MyOptions>();
        services.AddPlatformOptionsHotReload<MyOptions>();

        var provider = services.BuildServiceProvider();

        var logger = provider.GetService<ConfigurationChangeLogger>();
        logger.Should().NotBeNull();
    }

    [Fact]
    public void AddPlatformOptionsHotReload_ShouldRegisterConfigurationValidator()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions<MyOptions>();
        services.AddPlatformOptionsHotReload<MyOptions>();

        var provider = services.BuildServiceProvider();

        var validator = provider.GetService<ConfigurationValidator<MyOptions>>();
        validator.Should().NotBeNull();
    }

    [Fact]
    public void AddPlatformOptionsHotReload_CalledTwice_ShouldNotDuplicateRegistrations()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions<MyOptions>();
        services.AddPlatformOptionsHotReload<MyOptions>();
        services.AddPlatformOptionsHotReload<MyOptions>();

        var registrations = services
            .Where(sd => sd.ServiceType == typeof(IOptionsHotReload<MyOptions>))
            .Count();

        registrations.Should().Be(1);
    }

    [Fact]
    public void AddPlatformOptionsHotReload_WithNullServices_ShouldThrowArgumentNullException()
    {
        IServiceCollection? services = null;

        var act = () => services!.AddPlatformOptionsHotReload<MyOptions>();

        act.Should().Throw<ArgumentNullException>();
    }
}
