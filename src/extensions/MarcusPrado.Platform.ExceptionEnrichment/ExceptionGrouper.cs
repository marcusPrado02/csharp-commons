namespace MarcusPrado.Platform.ExceptionEnrichment;

/// <summary>
/// Groups a collection of exceptions by their fingerprint, as produced by
/// <see cref="ExceptionFingerprinter.GetFingerprint"/>.
/// </summary>
public static class ExceptionGrouper
{
    /// <summary>
    /// Groups <paramref name="exceptions"/> by fingerprint and returns a dictionary
    /// mapping each fingerprint to the list of exceptions that share it.
    /// </summary>
    /// <param name="exceptions">The exceptions to group.</param>
    /// <returns>
    /// A read-only dictionary whose keys are fingerprint hex strings and whose values
    /// are the exceptions that produced each fingerprint.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="exceptions"/> is <see langword="null"/>.
    /// </exception>
    public static IReadOnlyDictionary<string, IReadOnlyList<Exception>> GroupByFingerprint(
        IEnumerable<Exception> exceptions
    )
    {
        ArgumentNullException.ThrowIfNull(exceptions);

        var result = new Dictionary<string, List<Exception>>(StringComparer.Ordinal);

        foreach (var exception in exceptions)
        {
            var fingerprint = ExceptionFingerprinter.GetFingerprint(exception);

            if (!result.TryGetValue(fingerprint, out var list))
            {
                list = [];
                result[fingerprint] = list;
            }

            list.Add(exception);
        }

        return result.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<Exception>)kvp.Value.AsReadOnly(),
            StringComparer.Ordinal
        );
    }
}
