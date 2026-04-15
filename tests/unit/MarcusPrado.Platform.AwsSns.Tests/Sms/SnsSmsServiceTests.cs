namespace MarcusPrado.Platform.AwsSns.Tests.Sms;

public sealed class AwsSnsOptionsTests
{
    [Fact]
    public void DefaultRegion_IsUsEast1()
    {
        var opts = new AwsSnsOptions();
        opts.Region.Should().Be("us-east-1");
    }

    [Fact]
    public void DefaultSmsType_IsTransactional()
    {
        var opts = new AwsSnsOptions();
        opts.SmsType.Should().Be("Transactional");
    }

    [Fact]
    public void SenderId_DefaultsToNull()
    {
        var opts = new AwsSnsOptions();
        opts.SenderId.Should().BeNull();
    }

    [Fact]
    public void Configure_SetsAllProperties()
    {
        var opts = new AwsSnsOptions
        {
            Region = "eu-west-1",
            SenderId = "PLATFORM",
            SmsType = "Promotional",
        };

        opts.Region.Should().Be("eu-west-1");
        opts.SenderId.Should().Be("PLATFORM");
        opts.SmsType.Should().Be("Promotional");
    }
}

public sealed class SnsSmsServiceTests
{
    private static SnsSmsService BuildService(
        IAmazonSimpleNotificationService? sns = null, AwsSnsOptions? opts = null)
    {
        sns ??= Substitute.For<IAmazonSimpleNotificationService>();
        opts ??= new AwsSnsOptions();
        return new SnsSmsService(sns, opts);
    }

    [Fact]
    public void Constructor_WithNullSns_ThrowsArgumentNullException()
    {
        Action act = () => new SnsSmsService(null!, new AwsSnsOptions());
        act.Should().Throw<ArgumentNullException>().WithParameterName("sns");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        Action act = () => new SnsSmsService(Substitute.For<IAmazonSimpleNotificationService>(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public async Task SendAsync_WithNullMessage_ThrowsArgumentNullException()
    {
        var svc = BuildService();
        await Assert.ThrowsAsync<ArgumentNullException>(() => svc.SendAsync(null!));
    }

    [Fact]
    public async Task SendAsync_WhenSnsSucceeds_ReturnsSuccessWithMessageId()
    {
        var sns = Substitute.For<IAmazonSimpleNotificationService>();
        sns.PublishAsync(Arg.Any<PublishRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new PublishResponse { MessageId = "msg-001" }));

        var svc = BuildService(sns);
        var result = await svc.SendAsync(new SmsMessage("+15550001111", "Hello"));

        result.Success.Should().BeTrue();
        result.MessageId.Should().Be("msg-001");
    }

    [Fact]
    public async Task SendAsync_WhenSnsThrows_ReturnsFailureResult()
    {
        var sns = Substitute.For<IAmazonSimpleNotificationService>();
        sns.PublishAsync(Arg.Any<PublishRequest>(), Arg.Any<CancellationToken>())
            .Returns<PublishResponse>(_ => throw new AmazonSimpleNotificationServiceException("SNS error"));

        var svc = BuildService(sns);
        var result = await svc.SendAsync(new SmsMessage("+15550001111", "Hello"));

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("SNS error");
    }
}

public sealed class AwsSnsExtensionsTests
{
    [Fact]
    public void AddPlatformAwsSns_RegistersISmsService()
    {
        var services = new ServiceCollection();
        services.AddPlatformAwsSns(o => o.Region = "us-east-1");

        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<ISmsService>()
            .Should().BeOfType<SnsSmsService>();
    }
}
