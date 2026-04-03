using MarcusPrado.Platform.Twilio.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Twilio.Clients;

namespace MarcusPrado.Platform.Twilio.Tests.Sms;

public sealed class TwilioOptionsTests
{
    [Fact]
    public void DefaultValues_AreEmptyStrings()
    {
        var opts = new TwilioOptions();

        opts.AccountSid.Should().BeEmpty();
        opts.AuthToken.Should().BeEmpty();
        opts.DefaultFrom.Should().BeEmpty();
    }

    [Fact]
    public void Configure_SetsAllProperties()
    {
        var opts = new TwilioOptions
        {
            AccountSid = "ACXXXX",
            AuthToken  = "tok",
            DefaultFrom = "+15550001111",
        };

        opts.AccountSid.Should().Be("ACXXXX");
        opts.AuthToken.Should().Be("tok");
        opts.DefaultFrom.Should().Be("+15550001111");
    }
}

public sealed class TwilioExtensionsTests
{
    [Fact]
    public void AddPlatformTwilio_RegistersISmsService()
    {
        var services = new ServiceCollection();
        services.AddPlatformTwilio(o =>
        {
            o.AccountSid = "ACTEST";
            o.AuthToken  = "token";
        });

        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<MarcusPrado.Platform.Abstractions.Sms.ISmsService>()
            .Should().BeOfType<TwilioSmsService>();
    }
}

public sealed class TwilioSmsServiceTests
{
    [Fact]
    public void Constructor_WithMockClient_DoesNotCallTwilioInit()
    {
        // When a client is injected, TwilioClient.Init should NOT be called
        // (no exception expected despite empty credentials)
        var client = Substitute.For<ITwilioRestClient>();
        var opts   = new TwilioOptions { DefaultFrom = "+15550000000" };

        var act = () => new TwilioSmsService(opts, client);

        act.Should().NotThrow();
    }

    [Fact]
    public async Task SendAsync_WhenClientFaults_ReturnsFailureResult()
    {
        var client = Substitute.For<ITwilioRestClient>();
        var opts   = new TwilioOptions { DefaultFrom = "+15550000000" };
        var svc    = new TwilioSmsService(opts, client);

        // Twilio internals will throw because mock returns null response
        var result = await svc.SendAsync(new SmsMessage("+15551111111", "hello"));

        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }
}
