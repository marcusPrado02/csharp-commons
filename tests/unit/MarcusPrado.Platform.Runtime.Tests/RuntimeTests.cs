using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MarcusPrado.Platform.Runtime.Configuration;
using MarcusPrado.Platform.Runtime.Environment;
using MarcusPrado.Platform.Runtime.Lifecycle;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace MarcusPrado.Platform.Runtime.Tests;

public sealed class InstanceInfoTests
{
    [Fact]
    public void FromEnvironment_NullEnvVars_UsesDefaults()
    {
        var info = InstanceInfo.FromEnvironment();

        info.ServiceName.Should().NotBeNullOrEmpty();
        info.ServiceVersion.Should().NotBeNullOrEmpty();
        info.PodName.Should().NotBeNullOrEmpty();
        info.NodeName.Should().NotBeNullOrEmpty();
        info.Region.Should().NotBeNullOrEmpty();
    }
}

public sealed class RegionTests
{
    [Fact]
    public void Local_HasCorrectValue()
    {
        Region.Local.Value.Should().Be("local");
    }

    [Fact]
    public void FromEnvironment_NoEnvVar_ReturnsLocal()
    {
        System.Environment.SetEnvironmentVariable("REGION", null);

        var region = Region.FromEnvironment();

        region.Value.Should().Be("local");
    }

    [Fact]
    public void FromEnvironment_WithEnvVar_ReturnsEnvValue()
    {
        System.Environment.SetEnvironmentVariable("REGION", "eastus2");

        var region = Region.FromEnvironment();

        region.Value.Should().Be("eastus2");
        System.Environment.SetEnvironmentVariable("REGION", null);
    }
}

public sealed class ConfigurationKeyTests
{
    [Fact]
    public void SameNameSameType_Equal()
    {
        var k1 = new ConfigurationKey<string>("App:Name");
        var k2 = new ConfigurationKey<string>("App:Name");

        k1.Should().Be(k2);
    }

    [Fact]
    public void DifferentName_NotEqual()
    {
        var k1 = new ConfigurationKey<string>("App:Name");
        var k2 = new ConfigurationKey<string>("App:Version");

        k1.Should().NotBe(k2);
    }
}

public sealed class DeploymentEnvironmentTests
{
    [Fact]
    public void AllTiersAreDefined()
    {
        var values = Enum.GetValues<DeploymentEnvironment>();

        values.Should().Contain(DeploymentEnvironment.Development);
        values.Should().Contain(DeploymentEnvironment.Staging);
        values.Should().Contain(DeploymentEnvironment.Production);
    }
}

public sealed class EnvConfigurationTests
{
    private static IConfiguration BuildConfig(Dictionary<string, string?> values)
        => new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    [Fact]
    public void Get_ReadsFromConfiguration()
    {
        var config = BuildConfig(new() { ["App:Name"] = "TestService" });
        var envCfg = new EnvConfiguration(config);
        var key = new ConfigurationKey<string>("App:Name");

        var result = envCfg.Get(key);

        result.Should().Be("TestService");
    }

    [Fact]
    public void Get_EnvVarOverridesConfig()
    {
        System.Environment.SetEnvironmentVariable("APP__NAME", "EnvService");
        var config = BuildConfig(new() { ["App:Name"] = "ConfigService" });
        var envCfg = new EnvConfiguration(config);
        var key = new ConfigurationKey<string>("App:Name");

        var result = envCfg.Get(key);

        result.Should().Be("EnvService");
        System.Environment.SetEnvironmentVariable("APP__NAME", null);
    }

    [Fact]
    public void Get_MissingKey_ReturnsNull()
    {
        var envCfg = new EnvConfiguration(BuildConfig(new()));
        var key = new ConfigurationKey<string>("Missing:Key");

        var result = envCfg.Get(key);

        result.Should().BeNull();
    }

    [Fact]
    public void GetSection_BindsSection()
    {
        var config = BuildConfig(new()
        {
            ["Service:Name"] = "TestApp",
            ["Service:Version"] = "2.0.0",
        });
        var envCfg = new EnvConfiguration(config);

        var section = envCfg.GetSection<ServiceSettings>("Service");

        section.Name.Should().Be("TestApp");
        section.Version.Should().Be("2.0.0");
    }

    [Fact]
    public void GetSection_MissingSection_Throws()
    {
        var envCfg = new EnvConfiguration(BuildConfig(new()));

        var act = () => envCfg.GetSection<ServiceSettings>("NonExistent");

        act.Should().Throw<InvalidOperationException>();
    }

    private sealed class ServiceSettings
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
    }
}

public sealed class GracefulShutdownTests
{
    [Fact]
    public async Task RunAsync_AllHandlersExecuted()
    {
        var executed = 0;
        var shutdown = new GracefulShutdown();
        shutdown.Register(_ => { executed++; return Task.CompletedTask; });
        shutdown.Register(_ => { executed++; return Task.CompletedTask; });

        await shutdown.RunAsync();

        executed.Should().Be(2);
    }

    [Fact]
    public async Task RunAsync_NoHandlers_Completes()
    {
        var shutdown = new GracefulShutdown();

        var act = async () => await shutdown.RunAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RunAsync_CustomTimeout_IsRespected()
    {
        var shutdown = new GracefulShutdown(TimeSpan.FromSeconds(5));
        var started = false;

        shutdown.Register(async ct =>
        {
            started = true;
            await Task.Delay(10, ct);
        });

        await shutdown.RunAsync();

        started.Should().BeTrue();
    }
}
