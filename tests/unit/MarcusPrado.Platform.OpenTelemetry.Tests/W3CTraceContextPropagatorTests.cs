using System.Diagnostics;
using MarcusPrado.Platform.Observability.Tracing;

namespace MarcusPrado.Platform.OpenTelemetry.Tests;

/// <summary>Unit tests for <see cref="W3CTraceContextPropagator"/>.</summary>
public sealed class W3CTraceContextPropagatorTests
{
    // ── Inject ────────────────────────────────────────────────────────────────

    [Fact]
    public void Inject_WithActiveActivity_SetsTraceparentHeader()
    {
        using var activity = CreateActivity();
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        W3CTraceContextPropagator.Inject(activity, headers, (h, k, v) => h[k] = v);

        headers.Should().ContainKey("traceparent");
        headers["traceparent"].Should().MatchRegex(@"^00-[0-9a-f]{32}-[0-9a-f]{16}-\d{2}$");
    }

    [Fact]
    public void Inject_WithNullActivity_DoesNotThrow()
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var act = () => W3CTraceContextPropagator.Inject(null, headers, (h, k, v) => h[k] = v);
        act.Should().NotThrow();
        headers.Should().BeEmpty();
    }

    [Fact]
    public void Inject_WithTraceState_SetsTracestateHeader()
    {
        using var activity = CreateActivity();
        activity.TraceStateString = "vendor=abc";
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        W3CTraceContextPropagator.Inject(activity, headers, (h, k, v) => h[k] = v);

        headers.Should().ContainKey("tracestate");
        headers["tracestate"].Should().Be("vendor=abc");
    }

    // ── Extract ───────────────────────────────────────────────────────────────

    [Fact]
    public void Extract_WithValidTraceparent_ReturnsParsedContext()
    {
        using var original = CreateActivity();
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        W3CTraceContextPropagator.Inject(original, headers, (h, k, v) => h[k] = v);

        var ctx = W3CTraceContextPropagator.Extract(headers,
            (h, k) => h.TryGetValue(k, out var v) ? v : null);

        ctx.TraceId.Should().Be(original.TraceId);
        ctx.SpanId.Should().Be(original.SpanId);
    }

    [Fact]
    public void Extract_WithMissingHeader_ReturnsDefault()
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var ctx = W3CTraceContextPropagator.Extract(headers,
            (h, k) => h.TryGetValue(k, out var v) ? v : null);

        ctx.Should().Be(default(ActivityContext));
    }

    [Fact]
    public void Extract_WithInvalidTraceparent_ReturnsDefault()
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["traceparent"] = "not-a-valid-traceparent",
        };

        var ctx = W3CTraceContextPropagator.Extract(headers,
            (h, k) => h.TryGetValue(k, out var v) ? v : null);

        ctx.Should().Be(default(ActivityContext));
    }

    // ── ActivityExtensions ────────────────────────────────────────────────────

    [Fact]
    public void SetTenantId_SetsTagOnActivity()
    {
        using var activity = CreateActivity();
        activity.SetTenantId("tenant-abc");
        activity.GetTagItem("tenant.id").Should().Be("tenant-abc");
    }

    [Fact]
    public void SetUserId_SetsTagOnActivity()
    {
        using var activity = CreateActivity();
        activity.SetUserId("user-xyz");
        activity.GetTagItem("user.id").Should().Be("user-xyz");
    }

    [Fact]
    public void SetCorrelationId_SetsTagOnActivity()
    {
        using var activity = CreateActivity();
        activity.SetCorrelationId("corr-123");
        activity.GetTagItem("correlation.id").Should().Be("corr-123");
    }

    [Fact]
    public void SetErrorStatus_WithException_SetsErrorStatusAndTags()
    {
        using var activity = CreateActivity();
        var ex = new InvalidOperationException("boom");

        activity.SetErrorStatus(ex);

        activity.Status.Should().Be(ActivityStatusCode.Error);
        activity.GetTagItem("exception.message").Should().Be("boom");
    }

    [Fact]
    public void SetTenantId_WithNullActivity_ReturnsNull()
    {
        Activity? activity = null;
        var result = activity.SetTenantId("t");
        result.Should().BeNull();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Activity CreateActivity()
    {
        var source = new ActivitySource("TestSource");
        var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
        };
        ActivitySource.AddActivityListener(listener);

        return source.StartActivity("TestOp") ?? new Activity("TestOp").Start();
    }
}
