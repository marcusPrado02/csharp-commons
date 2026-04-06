using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MarcusPrado.Platform.Degradation;

/// <summary>
/// Provides minimal-API endpoint mapping for the degradation control surface.
/// </summary>
public static class DegradationEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Maps the degradation status and mode-change endpoints:
    /// <list type="bullet">
    /// <item><c>GET  /degradation/status</c> — returns the current mode as JSON.</item>
    /// <item><c>POST /degradation/mode</c>   — accepts <c>{ "mode": "ReadOnly" }</c> and sets the mode.</item>
    /// </list>
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add routes to.</param>
    /// <returns>The same <see cref="IEndpointRouteBuilder"/> for chaining.</returns>
    public static IEndpointRouteBuilder MapDegradationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/degradation/status", async (
            IDegradationController controller,
            CancellationToken ct) =>
        {
            var mode = await controller.GetModeAsync(ct);
            return Results.Ok(new { mode = mode.ToString() });
        });

        endpoints.MapPost("/degradation/mode", async (
            HttpContext httpContext,
            IDegradationController controller,
            CancellationToken ct) =>
        {
            SetModeRequest? request;
            try
            {
                request = await JsonSerializer.DeserializeAsync<SetModeRequest>(
                    httpContext.Request.Body, JsonOptions, ct);
            }
            catch (JsonException)
            {
                return Results.BadRequest(new { error = "Invalid JSON body." });
            }

            if (request is null)
                return Results.BadRequest(new { error = "Request body is required." });

            if (!Enum.TryParse<DegradationMode>(request.Mode, ignoreCase: true, out var parsedMode))
                return Results.BadRequest(new { error = $"Unknown degradation mode: '{request.Mode}'." });

            await controller.SetModeAsync(parsedMode, ct);
            return Results.Ok(new { mode = parsedMode.ToString() });
        });

        return endpoints;
    }

    // ── Private DTOs ──────────────────────────────────────────────────────────

    private sealed class SetModeRequest
    {
        /// <summary>Gets the degradation mode name.</summary>
        public string? Mode { get; }

        /// <summary>Initialises a new <see cref="SetModeRequest"/>.</summary>
        [JsonConstructor]
        public SetModeRequest(string? mode) => Mode = mode;
    }
}
