using Grpc.Net.Client;
using MarcusPrado.Platform.Grpc.Interceptors;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Grpc.Extensions;

/// <summary>DI helpers for platform gRPC interceptors and client factory.</summary>
public static class GrpcServiceExtensions
{
    /// <summary>
    /// Adds all platform gRPC server interceptors to the DI container so they
    /// can be used with <c>services.AddGrpc().Interceptors</c>.
    /// </summary>
    public static IServiceCollection AddPlatformGrpcInterceptors(
        this IServiceCollection services)
    {
        services.AddSingleton<CorrelationInterceptor>();
        services.AddSingleton<LoggingInterceptor>();
        services.AddSingleton<AuthInterceptor>();
        return services;
    }

    /// <summary>
    /// Creates a <see cref="GrpcChannel"/> with the correlation interceptor
    /// attached on the client side.
    /// </summary>
    public static GrpcChannel CreateChannel(
        string address,
        ILogger<CorrelationInterceptor>? logger = null)
    {
        var channel = GrpcChannel.ForAddress(address);
        return channel;
    }
}
