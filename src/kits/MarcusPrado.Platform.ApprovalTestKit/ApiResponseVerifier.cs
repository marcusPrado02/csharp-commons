namespace MarcusPrado.Platform.ApprovalTestKit;

/// <summary>
/// Snapshots an <see cref="HttpResponseMessage"/> into a <see cref="VerifySnapshot"/> record
/// that contains the HTTP status code and the (scrubbed) response body.
/// </summary>
public static class ApiResponseVerifier
{
    /// <summary>
    /// Reads the response body, applies all scrubbers from <paramref name="settings"/>,
    /// and returns a <see cref="VerifySnapshot"/> containing the status code and the scrubbed body.
    /// </summary>
    /// <param name="response">The HTTP response message to snapshot.</param>
    /// <param name="settings">
    /// The <see cref="PlatformVerifySettings"/> containing the scrubbers to apply.
    /// Pass <see langword="null"/> to use <see cref="PlatformVerifySettings.CreateDefault"/>.
    /// </param>
    /// <returns>A task that resolves to the populated <see cref="VerifySnapshot"/>.</returns>
    public static async Task<VerifySnapshot> SnapshotAsync(
        HttpResponseMessage response,
        PlatformVerifySettings? settings = null)
    {
        ArgumentNullException.ThrowIfNull(response);

        var effectiveSettings = settings ?? PlatformVerifySettings.CreateDefault();
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var scrubbedBody = effectiveSettings.Apply(body);

        return new VerifySnapshot((int)response.StatusCode, scrubbedBody);
    }
}
