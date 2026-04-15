using SendGrid.Helpers.Mail;

namespace MarcusPrado.Platform.SendGrid.Tests.Email;

public sealed class SendGridOptionsTests
{
    [Fact]
    public void DefaultApiKey_IsEmpty()
    {
        var opts = new SendGridOptions();
        opts.ApiKey.Should().BeEmpty();
    }

    [Fact]
    public void DefaultFrom_IsNotEmpty()
    {
        var opts = new SendGridOptions();
        opts.DefaultFrom.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Configure_SetsApiKeyAndFrom()
    {
        var opts = new SendGridOptions
        {
            ApiKey = "SG.test",
            DefaultFrom = "sender@example.com",
            DefaultFromName = "Sender",
        };

        opts.ApiKey.Should().Be("SG.test");
        opts.DefaultFrom.Should().Be("sender@example.com");
        opts.DefaultFromName.Should().Be("Sender");
    }
}

public sealed class SendGridExtensionsTests
{
    [Fact]
    public void AddPlatformSendGrid_RegistersIEmailSender()
    {
        var services = new ServiceCollection();
        services.AddPlatformSendGrid(o => o.ApiKey = "SG.test");

        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IEmailSender>().Should().BeOfType<SendGridEmailSender>();
    }

    [Fact]
    public void AddPlatformSendGrid_RegistersISendGridClient()
    {
        var services = new ServiceCollection();
        services.AddPlatformSendGrid(o => o.ApiKey = "SG.test");

        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<ISendGridClient>().Should().NotBeNull();
    }

    [Fact]
    public void AddPlatformSendGrid_WithoutConfigure_UsesDefaults()
    {
        var services = new ServiceCollection();
        services.AddPlatformSendGrid();

        var sp = services.BuildServiceProvider();
        var opts = sp.GetRequiredService<SendGridOptions>();

        opts.Should().NotBeNull();
    }
}

public sealed class SendGridEmailSenderTests
{
    private static SendGridEmailSender BuildSender(ISendGridClient? client = null)
    {
        client ??= Substitute.For<ISendGridClient>();
        return new SendGridEmailSender(
            client,
            new SendGridOptions { DefaultFrom = "noreply@example.com", DefaultFromName = "Platform" }
        );
    }

    [Fact]
    public void Constructor_WithNullClient_ThrowsArgumentNullException()
    {
        Action act = () => new SendGridEmailSender(null!, new SendGridOptions());
        act.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        Action act = () => new SendGridEmailSender(Substitute.For<ISendGridClient>(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public async Task SendAsync_WithNullMessage_ThrowsArgumentNullException()
    {
        var sender = BuildSender();
        await Assert.ThrowsAsync<ArgumentNullException>(() => sender.SendAsync(null!));
    }

    [Fact]
    public async Task SendAsync_WhenApiReturnsSuccess_ReturnsSuccessResult()
    {
        var client = Substitute.For<ISendGridClient>();
        var httpMsg = new HttpResponseMessage(HttpStatusCode.Accepted);
        var response = new global::SendGrid.Response(HttpStatusCode.Accepted, httpMsg.Content, httpMsg.Headers);

        client
            .SendEmailAsync(Arg.Any<SendGridMessage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var sender = BuildSender(client);
        var message = new EmailMessage("to@example.com", "Subject", "Body");

        var result = await sender.SendAsync(message);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_WhenApiThrows_ReturnsFailureResult()
    {
        var client = Substitute.For<ISendGridClient>();
        client
            .SendEmailAsync(Arg.Any<SendGridMessage>(), Arg.Any<CancellationToken>())
            .Returns<global::SendGrid.Response>(_ => throw new InvalidOperationException("network error"));

        var sender = BuildSender(client);
        var message = new EmailMessage("to@example.com", "Subject", "Body");

        var result = await sender.SendAsync(message);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("network error");
    }

    [Fact]
    public async Task SendBulkAsync_EmptyList_ReturnsSuccessWithoutCallingApi()
    {
        var client = Substitute.For<ISendGridClient>();
        var sender = BuildSender(client);

        var result = await sender.SendBulkAsync(Array.Empty<EmailMessage>());

        result.Success.Should().BeTrue();
        await client.DidNotReceiveWithAnyArgs().SendEmailAsync(default!);
    }

    [Fact]
    public async Task SendBulkAsync_WithNullList_ThrowsArgumentNullException()
    {
        var sender = BuildSender();
        await Assert.ThrowsAsync<ArgumentNullException>(() => sender.SendBulkAsync(null!));
    }
}
