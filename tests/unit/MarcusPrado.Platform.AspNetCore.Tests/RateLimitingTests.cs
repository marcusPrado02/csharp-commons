using MarcusPrado.Platform.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.AspNetCore.Tests;

public sealed class RateLimitingTests
{
    [Fact]
    public void AddPlatformRateLimiting_Defaults_RegistersOptions()
    {
        var sp   = BuildServiceProvider();
        var opts = sp.GetService<PlatformRateLimitingOptions>();

        opts.Should().NotBeNull();
        opts!.TenantPermitLimit.Should().Be(200);
        opts.UserPermitLimit.Should().Be(60);
    }

    [Fact]
    public void AddPlatformRateLimiting_Configure_OverridesOptions()
    {
        var sp = new ServiceCollection()
            .AddLogging()
            .AddPlatformRateLimiting(o =>
            {
                o.TenantPermitLimit = 50;
                o.UserPermitLimit   = 10;
            })
            .BuildServiceProvider();

        var opts = sp.GetRequiredService<PlatformRateLimitingOptions>();
        opts.TenantPermitLimit.Should().Be(50);
        opts.UserPermitLimit.Should().Be(10);
    }

    [Fact]
    public void TenantPolicy_NoHeader_UsesAnonKey()
    {
        var policy    = new TenantRateLimitPolicy(new PlatformRateLimitingOptions());
        var partition = policy.GetPartition(new DefaultHttpContext());

        partition.PartitionKey.Should().Be("__anon__");
    }

    [Fact]
    public void TenantPolicy_WithHeader_UsesTenantIdAsKey()
    {
        var policy  = new TenantRateLimitPolicy(new PlatformRateLimitingOptions());
        var ctx     = new DefaultHttpContext();
        ctx.Request.Headers["X-Tenant-ID"] = "tenant-42";

        var partition = policy.GetPartition(ctx);

        partition.PartitionKey.Should().Be("tenant-42");
    }

    [Fact]
    public void UserPolicy_Anonymous_UsesAnonKey()
    {
        var policy    = new UserRateLimitPolicy(new PlatformRateLimitingOptions());
        var partition = policy.GetPartition(new DefaultHttpContext());

        partition.PartitionKey.Should().Be("__anon__");
    }

    [Fact]
    public void TenantPolicy_OnRejected_IsNotNull()
        => new TenantRateLimitPolicy(new PlatformRateLimitingOptions())
            .OnRejected.Should().NotBeNull();

    [Fact]
    public void UserPolicy_OnRejected_IsNotNull()
        => new UserRateLimitPolicy(new PlatformRateLimitingOptions())
            .OnRejected.Should().NotBeNull();

    private static IServiceProvider BuildServiceProvider()
        => new ServiceCollection()
            .AddLogging()
            .AddPlatformRateLimiting()
            .BuildServiceProvider();
}
