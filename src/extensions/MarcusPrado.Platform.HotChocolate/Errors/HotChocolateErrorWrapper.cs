using MarcusPrado.Platform.Abstractions.GraphQL;
using HC = global::HotChocolate;

namespace MarcusPrado.Platform.HotChocolate.Errors;

/// <summary>Wraps a HotChocolate <see cref="HC.IError"/> as a platform <see cref="IGraphQlError"/>.</summary>
internal sealed class HotChocolateErrorWrapper : IGraphQlError
{
    private readonly HC.IError _inner;

    internal HotChocolateErrorWrapper(HC.IError inner) => _inner = inner;

    /// <inheritdoc />
    public string Message => _inner.Message;

    /// <inheritdoc />
    public string? Code => _inner.Code;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?>? Extensions =>
        _inner.Extensions?.ToDictionary(kv => kv.Key, kv => kv.Value);
}
