using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MarcusPrado.Platform.DlqReprocessing.Endpoints;

/// <summary>
/// Provides minimal-API endpoint mapping for the DLQ inspection and reprocessing surface.
/// </summary>
public static class DlqEndpoints
{
    /// <summary>
    /// Maps DLQ endpoints onto the given <paramref name="app"/>:
    /// <list type="bullet">
    /// <item><c>GET  /dlq/{topic}</c>                    — lists all messages for the topic.</item>
    /// <item><c>POST /dlq/{topic}/reprocess/{id}</c>     — requeues a single message (200 or 404).</item>
    /// <item><c>DELETE /dlq/{topic}/{id}</c>             — permanently deletes a message (204 or 404).</item>
    /// </list>
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to add routes to.</param>
    /// <returns>The same <see cref="IEndpointRouteBuilder"/> for chaining.</returns>
    public static IEndpointRouteBuilder MapDlqEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapGet(
            "/dlq/{topic}",
            async (string topic, IDlqStore store, CancellationToken ct) =>
            {
                var messages = await store.GetAsync(topic, ct).ConfigureAwait(false);
                return Results.Ok(messages);
            }
        );

        app.MapPost(
            "/dlq/{topic}/reprocess/{id}",
            async (string topic, string id, IDlqStore store, IDlqMetrics metrics, CancellationToken ct) =>
            {
                var message = await store.GetByIdAsync(topic, id, ct).ConfigureAwait(false);
                if (message is null)
                    return Results.NotFound(new { error = $"Message '{id}' not found in topic '{topic}'." });

                await store.RequeueAsync(topic, id, ct).ConfigureAwait(false);
                metrics.RecordReprocessed(topic);
                return Results.Ok(new { requeued = id });
            }
        );

        app.MapDelete(
            "/dlq/{topic}/{id}",
            async (string topic, string id, IDlqStore store, IDlqMetrics metrics, CancellationToken ct) =>
            {
                var message = await store.GetByIdAsync(topic, id, ct).ConfigureAwait(false);
                if (message is null)
                    return Results.NotFound(new { error = $"Message '{id}' not found in topic '{topic}'." });

                await store.DeleteAsync(topic, id, ct).ConfigureAwait(false);
                metrics.RecordDeleted(topic);
                return Results.NoContent();
            }
        );

        return app;
    }
}
