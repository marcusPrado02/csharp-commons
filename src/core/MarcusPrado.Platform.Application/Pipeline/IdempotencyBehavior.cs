using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using MarcusPrado.Platform.Abstractions.Errors;
using MarcusPrado.Platform.Abstractions.Results;
using MarcusPrado.Platform.Application.Idempotency;

namespace MarcusPrado.Platform.Application.Pipeline;

/// <summary>
/// Short-circuits duplicate requests for commands decorated with <see cref="IdempotentAttribute"/>.
/// On a cache hit the stored response is returned without invoking the handler.
/// Registered as the sixth behavior (order 6).
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IIdempotencyStore? _store;

    /// <summary>Initializes the behavior; <paramref name="store"/> may be null when not registered.</summary>
    public IdempotencyBehavior(IIdempotencyStore? store = null)
    {
        _store = store;
    }

    /// <inheritdoc/>
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        if (_store is null)
        {
            return await next(cancellationToken);
        }

        var attribute = typeof(TRequest)
            .GetCustomAttributes(typeof(IdempotentAttribute), inherit: false)
            .OfType<IdempotentAttribute>()
            .FirstOrDefault();

        if (attribute is null)
        {
            return await next(cancellationToken);
        }

        var key = request is IHaveIdempotencyKey keyed
            ? keyed.IdempotencyKey
            : BuildHashKey(request);

        var (found, serialized) = await _store.TryGetAsync(key, cancellationToken);

        if (found && serialized is not null)
        {
            return SafeDeserialize(serialized);
        }

        var response = await next(cancellationToken);

        var json = SafeSerialize(response);
        await _store.SetAsync(key, json, TimeSpan.FromSeconds(attribute.TimeToLiveSeconds), cancellationToken);

        return response;
    }

    // ── Result-safe serialisation ────────────────────────────────────────────

    private static string SafeSerialize(TResponse response)
    {
        var type = typeof(TResponse);

        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Result<>))
        {
            return JsonSerializer.Serialize(response);
        }

        var isSuccess = (bool)type.GetProperty("IsSuccess")!.GetValue(response)!;
        var node = new JsonObject { ["s"] = isSuccess };

        if (isSuccess)
        {
            var value = type.GetProperty("Value")!.GetValue(response);
            node["v"] = value is null ? null : JsonSerializer.SerializeToNode(value);
        }
        else
        {
            var error = (Error)type.GetProperty("Error")!.GetValue(response)!;
            node["ec"] = error.Code;
            node["em"] = error.Message;
            node["cat"] = (int)error.Category;
        }

        return node.ToJsonString();
    }

    private static TResponse SafeDeserialize(string json)
    {
        var type = typeof(TResponse);

        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Result<>))
        {
            return JsonSerializer.Deserialize<TResponse>(json)!;
        }

        var node = JsonNode.Parse(json)!.AsObject();
        var sNode = node["s"];
        var isSuccess = sNode!.GetValue<bool>();
        var valueType = type.GetGenericArguments()[0];

        if (isSuccess)
        {
            var vNode = node["v"];
            var value = vNode?.Deserialize(valueType);
            var successMethod = typeof(Result<>)
                .MakeGenericType(valueType)
                .GetMethod("Success", BindingFlags.Public | BindingFlags.Static)!;
            return (TResponse)successMethod.Invoke(null, new object?[] { value })!;
        }
        else
        {
            var ecNode = node["ec"];
            var emNode = node["em"];
            var catNode = node["cat"];
            var code = ecNode!.GetValue<string>();
            var message = emNode!.GetValue<string>();
            var category = (ErrorCategory)catNode!.GetValue<int>();
            var error = new Error(code, message, category);
            var failureMethod = typeof(Result)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m =>
                    m.Name == "Failure"
                    && m.IsGenericMethod
                    && m.GetParameters() is { Length: 1 } ps
                    && ps[0].ParameterType == typeof(Error))
                .MakeGenericMethod(valueType);
            return (TResponse)failureMethod.Invoke(null, new object[] { error })!;
        }
    }

    private static string BuildHashKey(TRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        var hash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(json));
        return $"{typeof(TRequest).FullName}:{Convert.ToHexString(hash)}";
    }
}
