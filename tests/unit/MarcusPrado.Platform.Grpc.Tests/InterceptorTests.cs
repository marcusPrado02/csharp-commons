namespace MarcusPrado.Platform.Grpc.Tests;

/// <summary>
/// Unit tests for gRPC interceptors — verify they can be instantiated and
/// basic preconditions without running a real gRPC server.
/// </summary>
public sealed class InterceptorTests
{
    [Fact]
    public void CorrelationInterceptor_CanBeInstantiated()
    {
        var log = NullLogger<CorrelationInterceptor>.Instance;
        var sut = new CorrelationInterceptor(log);
        sut.Should().NotBeNull();
    }

    [Fact]
    public void LoggingInterceptor_CanBeInstantiated()
    {
        var log = NullLogger<LoggingInterceptor>.Instance;
        var sut = new LoggingInterceptor(log);
        sut.Should().NotBeNull();
    }

    [Fact]
    public void AuthInterceptor_CanBeInstantiated()
    {
        var log = NullLogger<AuthInterceptor>.Instance;
        var sut = new AuthInterceptor(log);
        sut.Should().NotBeNull();
    }

    [Fact]
    public void CorrelationInterceptor_Constants_AreCorrect()
    {
        CorrelationInterceptor.CorrelationIdKey.Should().Be("x-correlation-id");
        CorrelationInterceptor.TenantIdKey.Should().Be("x-tenant-id");
    }

    [Fact]
    public void AllInterceptors_AreSubclassOfInterceptor()
    {
        typeof(CorrelationInterceptor).BaseType.Should().Be(typeof(Interceptor));
        typeof(LoggingInterceptor).BaseType.Should().Be(typeof(Interceptor));
        typeof(AuthInterceptor).BaseType.Should().Be(typeof(Interceptor));
    }
}
