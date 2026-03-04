namespace MarcusPrado.Platform.Grpc.Interceptors;

/// <summary>gRPC interceptor that logs incoming requests and durations.</summary>
public sealed partial class LoggingInterceptor : Interceptor
{
    private readonly ILogger<LoggingInterceptor> _logger;

    public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
        => _logger = logger;

    [LoggerMessage(Level = LogLevel.Information, Message = "gRPC [{Method}] started")]
    private static partial void LogStarted(ILogger logger, string method);

    [LoggerMessage(Level = LogLevel.Information, Message = "gRPC [{Method}] completed in {ElapsedMs}ms")]
    private static partial void LogCompleted(ILogger logger, string method, double elapsedMs);

    [LoggerMessage(Level = LogLevel.Error, Message = "gRPC [{Method}] failed in {ElapsedMs}ms")]
    private static partial void LogFailed(ILogger logger, Exception ex, string method, double elapsedMs);

    [LoggerMessage(Level = LogLevel.Information, Message = "gRPC streaming [{Method}] started")]
    private static partial void LogStreamingStarted(ILogger logger, string method);

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var start = DateTimeOffset.UtcNow;
        LogStarted(_logger, context.Method);

        try
        {
            var response = await continuation(request, context).ConfigureAwait(false);
            var ms = (DateTimeOffset.UtcNow - start).TotalMilliseconds;
            LogCompleted(_logger, context.Method, ms);
            return response;
        }
#pragma warning disable S2139 // Intentional: interceptor must log and propagate gRPC exceptions
        catch (Exception ex)
        {
            var ms = (DateTimeOffset.UtcNow - start).TotalMilliseconds;
            LogFailed(_logger, ex, context.Method, ms);
            throw;
        }
#pragma warning restore S2139
    }

    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        LogStreamingStarted(_logger, context.Method);
        return await continuation(requestStream, context).ConfigureAwait(false);
    }
}
