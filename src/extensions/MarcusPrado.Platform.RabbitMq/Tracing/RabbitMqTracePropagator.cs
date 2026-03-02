using System.Diagnostics;
using System.Text;
using MarcusPrado.Platform.Observability.Tracing;
using RabbitMQ.Client;

namespace MarcusPrado.Platform.RabbitMq.Tracing;

/// <summary>
/// Propagates W3C TraceContext via RabbitMQ <see cref="IReadOnlyBasicProperties"/> headers.
/// </summary>
public static class RabbitMqTracePropagator
{
    /// <summary>
    /// Injects the current activity's trace context into the given dictionary
    /// (use as the <c>BasicProperties.Headers</c> dictionary).
    /// </summary>
    public static void Inject(Activity? activity, IDictionary<string, object?> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        W3CTraceContextPropagator.Inject(activity, headers, static (h, key, value) =>
        {
            h[key] = Encoding.UTF8.GetBytes(value);
        });
    }

    /// <summary>
    /// Extracts a <see cref="ActivityContext"/> from the RabbitMQ message headers.
    /// </summary>
    public static ActivityContext Extract(IReadOnlyDictionary<string, object?>? headers)
    {
        if (headers is null)
        {
            return default;
        }

        return W3CTraceContextPropagator.Extract(headers, static (h, key) =>
        {
            if (!h.TryGetValue(key, out var raw))
            {
                return null;
            }

            return raw switch
            {
                byte[] bytes  => Encoding.UTF8.GetString(bytes),
                string s      => s,
                _             => null,
            };
        });
    }
}
