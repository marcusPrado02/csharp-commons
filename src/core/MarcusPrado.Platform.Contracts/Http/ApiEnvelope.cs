namespace MarcusPrado.Platform.Contracts.Http;

/// <summary>Non-generic base wrapper for API responses.</summary>
public class ApiEnvelope
{
    /// <summary>Whether the operation succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>ISO-8601 UTC timestamp.</summary>
    public string Timestamp { get; init; } = DateTimeOffset.UtcNow.ToString("O");

    /// <summary>Creates a successful generic envelope wrapping <paramref name="data"/>.</summary>
    public static ApiEnvelope<T> Ok<T>(T data)
        where T : class
        => new() { Success = true, Data = data };

    /// <summary>Creates a failure envelope with an error message.</summary>
    public static ApiEnvelope<object?> Fail(string error)
        => new() { Success = false, ErrorMessage = error };
}

/// <summary>Generic wrapper for API responses.</summary>
/// <typeparam name="T">The response payload type.</typeparam>
public sealed class ApiEnvelope<T> : ApiEnvelope
{
    /// <summary>The response payload.</summary>
    public T? Data { get; init; }

    /// <summary>Error message when <see cref="ApiEnvelope.Success"/> is <c>false</c>.</summary>
    public string? ErrorMessage { get; init; }
}
