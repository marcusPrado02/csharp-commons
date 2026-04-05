using FluentAssertions;
using MarcusPrado.Platform.HealthChecks.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace MarcusPrado.Platform.HealthChecks.Tests;

public sealed class StartupVerificationTests
{
    // ── VerificationResult ────────────────────────────────────────────────────

    [Fact]
    public void VerificationResult_SuccessTrue_HasNoErrorMessage()
    {
        var result = new VerificationResult(true, "test", null);

        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void VerificationResult_SuccessFalse_HasErrorMessage()
    {
        var result = new VerificationResult(false, "test", "something went wrong");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("something went wrong");
    }

    // ── DatabaseConnectivityVerification ──────────────────────────────────────

    [Fact]
    public async Task DatabaseConnectivity_ProbeReturnsTrue_ReturnsSuccess()
    {
        var verification = new DatabaseConnectivityVerification("db", () => Task.FromResult(true));

        var result = await verification.VerifyAsync(CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Name.Should().Be("db");
    }

    [Fact]
    public async Task DatabaseConnectivity_ProbeReturnsFalse_ReturnsFailure()
    {
        var verification = new DatabaseConnectivityVerification("db", () => Task.FromResult(false));

        var result = await verification.VerifyAsync(CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task DatabaseConnectivity_ProbeThrowsInvalidOperation_ReturnsFailure()
    {
        var verification = new DatabaseConnectivityVerification(
            "db",
            () => Task.FromException<bool>(new InvalidOperationException("connection refused")));

        var result = await verification.VerifyAsync(CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("connection refused");
    }

    [Fact]
    public void DatabaseConnectivity_NullProbe_ThrowsArgumentNullException()
    {
        var act = () => new DatabaseConnectivityVerification("db", null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── RequiredSecretsVerification ───────────────────────────────────────────

    [Fact]
    public async Task RequiredSecrets_AllKeysPresent_ReturnsSuccess()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["MySecret"] = "value" })
            .Build();

        var verification = new RequiredSecretsVerification(["MySecret"], config);

        var result = await verification.VerifyAsync(CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task RequiredSecrets_MissingKey_ReturnsFailureWithKeyName()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var verification = new RequiredSecretsVerification(["MissingKey"], config);

        var result = await verification.VerifyAsync(CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("MissingKey");
    }

    [Fact]
    public async Task RequiredSecrets_EmptyKeyValue_ReturnsFailure()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["EmptyKey"] = "" })
            .Build();

        var verification = new RequiredSecretsVerification(["EmptyKey"], config);

        var result = await verification.VerifyAsync(CancellationToken.None);

        result.Success.Should().BeFalse();
    }

    // ── StartupVerificationHostedService ─────────────────────────────────────

    [Fact]
    public async Task HostedService_AllVerificationsPass_DoesNotStopApplication()
    {
        var lifetime = Substitute.For<IHostApplicationLifetime>();
        var logger   = Substitute.For<ILogger<StartupVerificationHostedService>>();

        var passing = Substitute.For<IStartupVerification>();
        passing.Name.Returns("pass");
        passing.VerifyAsync(Arg.Any<CancellationToken>())
               .Returns(new VerificationResult(true, "pass", null));

        var service = new StartupVerificationHostedService([passing], lifetime, logger);

        await service.StartAsync(CancellationToken.None);

        lifetime.DidNotReceive().StopApplication();
    }

    [Fact]
    public async Task HostedService_OneVerificationFails_CallsStopApplication()
    {
        var lifetime = Substitute.For<IHostApplicationLifetime>();
        var logger   = Substitute.For<ILogger<StartupVerificationHostedService>>();

        var failing = Substitute.For<IStartupVerification>();
        failing.Name.Returns("fail");
        failing.VerifyAsync(Arg.Any<CancellationToken>())
               .Returns(new VerificationResult(false, "fail", "boom"));

        var service = new StartupVerificationHostedService([failing], lifetime, logger);

        await service.StartAsync(CancellationToken.None);

        lifetime.Received(1).StopApplication();
    }

    // ── DI extension methods ──────────────────────────────────────────────────

    [Fact]
    public void AddStartupVerification_RegistersHostedService()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(Substitute.For<IHostApplicationLifetime>());

        services.AddStartupVerification();

        var sp          = services.BuildServiceProvider();
        var hostedSvcs  = sp.GetServices<IHostedService>();
        hostedSvcs.Should().ContainSingle(s => s is StartupVerificationHostedService);
    }

    [Fact]
    public void AddDatabaseConnectivityVerification_RegistersIStartupVerification()
    {
        var services = new ServiceCollection();
        services.AddDatabaseConnectivityVerification(() => Task.FromResult(true));

        var sp            = services.BuildServiceProvider();
        var verifications = sp.GetServices<IStartupVerification>();
        verifications.Should().ContainSingle(v => v is DatabaseConnectivityVerification);
    }

    [Fact]
    public void AddRequiredSecretsVerification_RegistersIStartupVerification()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddRequiredSecretsVerification("SomeKey");

        var sp            = services.BuildServiceProvider();
        var verifications = sp.GetServices<IStartupVerification>();
        verifications.Should().ContainSingle(v => v is RequiredSecretsVerification);
    }
}
