using MarcusPrado.Platform.Observability.Correlation;

namespace MarcusPrado.Platform.Grpc.Interceptors;

/// <summary>
/// gRPC interceptor that propagates correlation and tenant IDs through
/// gRPC request metadata headers.
/// </summary>
public sealed partial class CorrelationInterceptor : Interceptor
{
    /// <summary>Metadata key for the correlation identifier.</summary>
    public const string CorrelationIdKey = "x-correlation-id";

    /// <summary>Metadata key for the tenant identifier.</summary>
    public const string TenantIdKey = "x-tenant-id";

    private readonly ILogger<CorrelationInterceptor> _logger;

    /// <summary>Initialises the interceptor with the given logger.</summary>
    public CorrelationInterceptor(ILogger<CorrelationInterceptor> logger)
        => _logger = logger;

    [LoggerMessage(Level = LogLevel.Debug, Message = "gRPC {Method} correlationId={CorrelationId} tenantId={TenantId}")]
    private static partial void LogCorrelation(ILogger logger, string method, string correlationId, string? tenantId);

    /// <inheritdoc />
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var correlationId = context.RequestHeaders.GetValue(CorrelationIdKey) ?? Guid.NewGuid().ToString();
        var tenantId = context.RequestHeaders.GetValue(TenantIdKey);

        LogCorrelation(_logger, context.Method, correlationId, tenantId);

        context.UserState[CorrelationIdKey] = correlationId;
        if (tenantId is not null)
        {
            context.UserState[TenantIdKey] = tenantId;
        }

        return await continuation(request, context).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var headers = context.Options.Headers ?? new Metadata();
        if (headers.Get(CorrelationIdKey) is null)
        {
            headers.Add(CorrelationIdKey, Guid.NewGuid().ToString());
        }

        var options = context.Options.WithHeaders(headers);
        var updated = new ClientInterceptorContext<TRequest, TResponse>(
            context.Method,
            context.Host,
            options);
        return continuation(request, updated);
    }
}
