namespace MarcusPrado.Platform.Abstractions.GraphQL;

/// <summary>Platform error filter, maps domain errors to GraphQL errors.</summary>
public interface IPlatformErrorFilter
{
    /// <summary>Transforms an unhandled exception into a typed GraphQL error.</summary>
    IGraphQlError OnError(IGraphQlError error, Exception exception);
}

/// <summary>Minimal representation of a GraphQL error.</summary>
public interface IGraphQlError
{
    /// <summary>Human-readable error message.</summary>
    string Message { get; }

    /// <summary>Machine-readable error code.</summary>
    string? Code { get; }

    /// <summary>Arbitrary extension data for the error.</summary>
    IReadOnlyDictionary<string, object?>? Extensions { get; }
}

/// <summary>Context available to every field resolver.</summary>
public interface IPlatformResolverContext
{
    /// <summary>The current tenant identifier.</summary>
    string? TenantId { get; }

    /// <summary>The current user identifier.</summary>
    string? UserId { get; }

    /// <summary>The current correlation identifier.</summary>
    string? CorrelationId { get; }

    /// <summary>Whether the current user is authenticated.</summary>
    bool IsAuthenticated { get; }
}
