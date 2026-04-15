using MarcusPrado.Platform.Stripe.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Stripe.Tests.Payment;

public sealed class StripeOptionsTests
{
    [Fact]
    public void DefaultApiKey_IsEmpty()
    {
        var opts = new StripeOptions();
        opts.ApiKey.Should().BeEmpty();
    }

    [Fact]
    public void Configure_SetsApiKey()
    {
        var opts = new StripeOptions { ApiKey = "sk_test_123" };
        opts.ApiKey.Should().Be("sk_test_123");
    }
}

public sealed class StripeExtensionsTests
{
    [Fact]
    public void AddPlatformStripe_RegistersPaymentService()
    {
        var services = new ServiceCollection();
        services.AddPlatformStripe(o => o.ApiKey = "sk_test_abc");

        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IPaymentService>().Should().BeOfType<StripePaymentService>();
    }

    [Fact]
    public void AddPlatformStripe_RegistersSubscriptionService()
    {
        var services = new ServiceCollection();
        services.AddPlatformStripe(o => o.ApiKey = "sk_test_abc");

        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<ISubscriptionService>().Should().BeOfType<StripeSubscriptionService>();
    }

    [Fact]
    public void AddPlatformStripe_RegistersStripeClient()
    {
        var services = new ServiceCollection();
        services.AddPlatformStripe(o => o.ApiKey = "sk_test_abc");

        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IStripeClient>().Should().NotBeNull();
    }
}

public sealed class StripePaymentServiceTests
{
    [Fact]
    public void Constructor_WithNullClient_ThrowsArgumentNullException()
    {
        Action act = () => new StripePaymentService(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithValidClient_Succeeds()
    {
        var client = Substitute.For<IStripeClient>();
        var act = () => new StripePaymentService(client);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task ChargeAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        var client = Substitute.For<IStripeClient>();
        var svc = new StripePaymentService(client);

        await Assert.ThrowsAsync<ArgumentNullException>(() => svc.ChargeAsync(null!));
    }
}

public sealed class StripeSubscriptionServiceTests
{
    [Fact]
    public void Constructor_WithNullClient_ThrowsArgumentNullException()
    {
        Action act = () => new StripeSubscriptionService(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithValidClient_Succeeds()
    {
        var client = Substitute.For<IStripeClient>();
        var act = () => new StripeSubscriptionService(client);
        act.Should().NotThrow();
    }
}
