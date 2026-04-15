namespace MarcusPrado.Platform.Grpc.Interceptors;

/// <summary>
/// gRPC interceptor that validates the <c>authorization</c> header is present
/// (acts as a gatekeeper before the handler is invoked).
/// </summary>
public sealed class AuthInterceptor : Interceptor
{
    private readonly ILogger<AuthInterceptor> _logger;

    /// <summary>Initializes a new instance of <see cref="AuthInterceptor"/> with the given logger.</summary>
    /// <param name="logger">Logger used to emit warnings when the authorization header is absent.</param>
    public AuthInterceptor(ILogger<AuthInterceptor> logger) => _logger = logger;

    /// <summary>
    /// Intercepts a unary call, rejects it with <see cref="StatusCode.Unauthenticated"/> when the
    /// <c>authorization</c> header is missing, and otherwise forwards to the next handler.
    /// </summary>
    /// <param name="request">The incoming request message.</param>
    /// <param name="context">The server call context providing access to request headers.</param>
    /// <param name="continuation">The next handler in the gRPC pipeline.</param>
    /// <returns>The response produced by the <paramref name="continuation"/>.</returns>
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation
    )
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
