using MarcusPrado.Platform.Abstractions.GraphQL;

namespace MarcusPrado.Platform.HotChocolate.Errors;

/// <summary>Immutable implementation of <see cref="IGraphQlError"/>.</summary>
public sealed class GraphQlError : IGraphQlError
{
    /// <summary>Initializes a new <see cref="GraphQlError"/>.</summary>
    public GraphQlError(string message, string? code = null, IReadOnlyDictionary<string, object?>? extensions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Message = message;
        Code = code;
        Extensions = extensions;
    }

    /// <inheritdoc />
    public string Message { get; }

    /// <inheritdoc />
    public string? Code { get; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?>? Extensions { get; }
}
