using System.Diagnostics;
using System.Text;
using Confluent.Kafka;
using MarcusPrado.Platform.Observability.Tracing;

namespace MarcusPrado.Platform.Kafka.Tracing;

/// <summary>
/// Propagates W3C TraceContext via Kafka <see cref="Message{TKey,TValue}"/> headers.
/// </summary>
public static class KafkaTracePropagator
{
    /// <summary>
    /// Injects the current activity's trace context into the Kafka message headers.
    /// </summary>
    public static void Inject(Activity? activity, Headers headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        W3CTraceContextPropagator.Inject(activity, headers, static (h, key, value) =>
        {
            h.Add(key, Encoding.UTF8.GetBytes(value));
        });
    }

    /// <summary>
    /// Extracts a <see cref="ActivityContext"/> from the Kafka message headers.
    /// </summary>
    public static ActivityContext Extract(Headers? headers)
    {
        if (headers is null)
        {
            return default;
        }

        return W3CTraceContextPropagator.Extract(headers, static (h, key) =>
        {
            try
            {
                var bytes = h.GetLastBytes(key);
                return bytes is null ? null : Encoding.UTF8.GetString(bytes);
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        });
    }
}
