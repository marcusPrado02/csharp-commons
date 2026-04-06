using System.Security.Cryptography;
using System.Text;

namespace MarcusPrado.Platform.ExceptionEnrichment;

/// <summary>
/// Produces a deterministic fingerprint (SHA-256 hex string) for an exception,
/// computed from the exception type name, message, and the first stack-trace frame.
/// Identical exceptions produce the same fingerprint, enabling error grouping.
/// </summary>
public static class ExceptionFingerprinter
{
    /// <summary>
    /// Returns a SHA-256 hex string computed from
    /// <c>"TypeName|Message|FirstFrame"</c>.
    /// </summary>
    /// <param name="exception">The exception to fingerprint.</param>
    /// <returns>A lowercase hexadecimal SHA-256 hash string.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="exception"/> is <see langword="null"/>.
    /// </exception>
    public static string GetFingerprint(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var typeName = exception.GetType().FullName ?? exception.GetType().Name;
        var message = exception.Message;
        var firstFrame = GetFirstFrame(exception);

        var raw = $"{typeName}|{message}|{firstFrame}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexStringLower(bytes);
    }

    private static string GetFirstFrame(Exception exception)
    {
        var stackTrace = exception.StackTrace;
        if (string.IsNullOrEmpty(stackTrace))
        {
            return string.Empty;
        }

        var lines = stackTrace.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        return lines.Length > 0 ? lines[0].Trim() : string.Empty;
    }
}
