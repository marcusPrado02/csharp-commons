namespace MarcusPrado.Platform.ApprovalTestKit;

/// <summary>
/// An immutable snapshot of an HTTP response, suitable for deterministic assertion.
/// </summary>
/// <param name="StatusCode">The HTTP status code as an integer (e.g. 200, 404).</param>
/// <param name="Body">The response body string, already scrubbed by any configured scrubbers.</param>
public sealed record VerifySnapshot(int StatusCode, string Body);
