namespace MarcusPrado.Platform.AspNetCore.Tests;

/// <summary>
/// Tests that <see cref="ServiceCollectionExtensions.AddPlatformCore"/> and
/// <see cref="ServiceCollectionExtensions.AddPlatformCqrs"/> register the
/// expected services in the DI container.
/// </summary>
public sealed class ServiceCollectionExtensionsTests
{
    // ── AddPlatformCore ───────────────────────────────────────────────────────

    [Fact]
    public void AddPlatformCore_ShouldRegister_IClock()
    {
        var sp = BuildServiceProvider();

        var clock = sp.GetService<IClock>();

        clock.Should().NotBeNull(because: "AddPlatformCore must register an IClock implementation");
        clock!.UtcNow.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AddPlatformCore_ShouldRegister_IGuidFactory()
    {
        var sp = BuildServiceProvider();

        var factory = sp.GetService<IGuidFactory>();

        factory.Should().NotBeNull(because: "AddPlatformCore must register an IGuidFactory implementation");
        factory!.NewGuid().Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void AddPlatformCore_ShouldRegister_IJsonSerializer()
    {
        var sp = BuildServiceProvider();

        var serializer = sp.GetService<IJsonSerializer>();

        serializer.Should().NotBeNull(because: "AddPlatformCore must register an IJsonSerializer implementation");
        var json = serializer!.Serialize(new { value = 42 });
        json.Should().Contain("42");
    }

    [Fact]
    public void AddPlatformCore_ShouldRegister_ICorrelationContext_AsScoped()
    {
        var sp = BuildServiceProvider();

        var descriptor = ((IServiceCollection)new ServiceCollection().AddPlatformCore()).FirstOrDefault(d =>
            d.ServiceType == typeof(ICorrelationContext)
        );

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddPlatformCore_ShouldRegister_ITenantContext_AsScoped()
    {
        var sp = BuildServiceProvider();

        var descriptor = ((IServiceCollection)new ServiceCollection().AddPlatformCore()).FirstOrDefault(d =>
            d.ServiceType == typeof(ITenantContext)
        );

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    // ── AddPlatformCqrs ───────────────────────────────────────────────────────

    [Fact]
    public void AddPlatformCqrs_ShouldRegister_AllEightPipelineBehaviors()
    {
        var services = new ServiceCollection();
        services.AddPlatformCqrs();

        var behaviors = services.Where(d => d.ServiceType == typeof(IPipelineBehavior<,>)).ToList();

        behaviors
            .Should()
            .HaveCount(
                8,
                because: "the platform CQRS pipeline must include all 8 standard behaviours: "
                    + "Validation, Authorization, Logging, Tracing, Metrics, Retry, Idempotency, Transaction"
            );
    }

    [Fact]
    public void AddPlatformCqrs_ShouldRegisterBehaviors_InCorrectOrder()
    {
        var services = new ServiceCollection();
        services.AddPlatformCqrs();

        var behaviorTypes = services
            .Where(d => d.ServiceType == typeof(IPipelineBehavior<,>))
            .Select(d => d.ImplementationType!.Name.Split('`')[0])
            .ToList();

        behaviorTypes[0].Should().Be("ValidationBehavior");
        behaviorTypes[1].Should().Be("AuthorizationBehavior");
        behaviorTypes[^1]
            .Should()
            .Be(
                "TransactionBehavior",
                because: "TransactionBehavior must be the last (innermost) decorator in the pipeline"
            );
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPlatformCore();
        return services.BuildServiceProvider();
    }
}
