using System.Diagnostics;

namespace MarcusPrado.Platform.Observability.Tracing;

/// <summary>
/// Provides helpers to inject and extract W3C TraceContext headers
/// (<c>traceparent</c> / <c>tracestate</c>) from arbitrary carriers such as
/// HTTP request headers and messaging message headers.
/// </summary>
public static class W3CTraceContextPropagator
{
    /// <summary>Header name for trace parent (W3C Trace Context Level 1).</summary>
    public const string TraceparentHeader = "traceparent";

    /// <summary>Header name for trace state (W3C Trace Context Level 1).</summary>
    public const string TracestateHeader = "tracestate";

    /// <summary>
    /// Injects the current <see cref="Activity"/> trace context into the
    /// carrier using the provided setter delegate.
    /// </summary>
    /// <typeparam name="TCarrier">The carrier type (e.g. <c>IBasicProperties</c>, <c>Headers</c>).</typeparam>
    /// <param name="activity">The activity whose context to propagate. If <c>null</c>, no-op.</param>
    /// <param name="carrier">The carrier to inject headers into.</param>
    /// <param name="setter">A delegate that sets a header key/value on the carrier.</param>
    public static void Inject<TCarrier>(
        Activity? activity,
        TCarrier carrier,
        Action<TCarrier, string, string> setter)
    {
        ArgumentNullException.ThrowIfNull(setter);

        if (activity is null)
        {
            return;
        }

        // W3C traceparent format: 00-{traceId}-{spanId}-{flags}
        var traceparent = $"00-{activity.TraceId}-{activity.SpanId}-{(activity.ActivityTraceFlags.HasFlag(ActivityTraceFlags.Recorded) ? "01" : "00")}";
        setter(carrier, TraceparentHeader, traceparent);

        if (!string.IsNullOrEmpty(activity.TraceStateString))
        {
            setter(carrier, TracestateHeader, activity.TraceStateString);
        }
    }

    /// <summary>
    /// Extracts a <see cref="ActivityContext"/> from the carrier using the
    /// provided getter delegate. Returns <see langword="default"/> if no
    /// valid trace context is found.
    /// </summary>
    /// <typeparam name="TCarrier">The carrier type.</typeparam>
    /// <param name="carrier">The carrier to extract headers from.</param>
    /// <param name="getter">A delegate that retrieves a header value by key from the carrier. Returns <c>null</c> when the header is absent.</param>
    /// <returns>The extracted <see cref="ActivityContext"/>, or <see langword="default"/> if extraction fails.</returns>
    public static ActivityContext Extract<TCarrier>(
        TCarrier carrier,
        Func<TCarrier, string, string?> getter)
    {
        ArgumentNullException.ThrowIfNull(getter);

        var traceparent = getter(carrier, TraceparentHeader);

        if (string.IsNullOrWhiteSpace(traceparent))
        {
            return default;
        }

        if (!ActivityContext.TryParse(traceparent, getter(carrier, TracestateHeader), out var ctx))
        {
            return default;
        }

        return ctx;
    }
}
