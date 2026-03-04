namespace MarcusPrado.Platform.Grpc.Interceptors;

/// <summary>
/// gRPC interceptor that validates the <c>authorization</c> header is present
/// (acts as a gatekeeper before the handler is invoked).
/// </summary>
public sealed class AuthInterceptor : Interceptor
{
    private readonly ILogger<AuthInterceptor> _logger;

    public AuthInterceptor(ILogger<AuthInterceptor> logger)
        => _logger = logger;

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var authHeader = context.RequestHeaders.GetValue("authorization");

        if (string.IsNullOrWhiteSpace(authHeader))
        {
            _logger.LogWarning("gRPC {Method} — authorization header missing", context.Method);
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing authorization header"));
        }

        return await continuation(request, context).ConfigureAwait(false);
    }
}
