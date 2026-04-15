using System.Net;
using FluentAssertions;
using MarcusPrado.Platform.ApprovalTestKit;
using Xunit;

namespace MarcusPrado.Platform.ApprovalTestKit.Tests;

// ── PlatformVerifySettings ────────────────────────────────────────────────────

public sealed class PlatformVerifySettingsTests
{
    [Fact]
    public void AddGuidScrubber_ReplacesGuidWithToken()
    {
        var settings = new PlatformVerifySettings().AddGuidScrubber();
        var input = $"id: {Guid.NewGuid()} end";

        var result = settings.Apply(input);

        result.Should().Be("id: «Guid» end");
    }

    [Fact]
    public void AddGuidScrubber_ReplacesMultipleGuids()
    {
        var settings = new PlatformVerifySettings().AddGuidScrubber();
        var g1 = Guid.NewGuid().ToString();
        var g2 = Guid.NewGuid().ToString();
        var input = $"{g1} and {g2}";

        var result = settings.Apply(input);

        result.Should().Be("«Guid» and «Guid»");
    }

    [Fact]
    public void AddDateTimeOffsetScrubber_ReplacesIso8601WithToken()
    {
        var settings = new PlatformVerifySettings().AddDateTimeOffsetScrubber();
        var input = "occurred: 2024-06-15T08:30:00.000+00:00 done";

        var result = settings.Apply(input);

        result.Should().Be("occurred: «DateTimeOffset» done");
    }

    [Fact]
    public void AddDateTimeOffsetScrubber_ReplacesZuluTimestamp()
    {
        var settings = new PlatformVerifySettings().AddDateTimeOffsetScrubber();
        var input = "ts=2023-12-01T23:59:59Z";

        var result = settings.Apply(input);

        result.Should().Be("ts=«DateTimeOffset»");
    }

    [Fact]
    public void AddCorrelationIdScrubber_ReplacesHeaderValue()
    {
        var settings = new PlatformVerifySettings().AddCorrelationIdScrubber();
        var guid = Guid.NewGuid().ToString();
        var input = $"X-Correlation-Id: {guid}";

        var result = settings.Apply(input);

        result.Should().Contain("«CorrelationId»");
        result.Should().NotContain(guid);
    }

    [Fact]
    public void Apply_NullInput_Throws()
    {
        var settings = new PlatformVerifySettings();
        var act = () => settings.Apply(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddScrubber_NullScrubber_Throws()
    {
        var settings = new PlatformVerifySettings();
        var act = () => settings.AddScrubber(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateDefault_AppliesAllThreeScrubbers()
    {
        var settings = PlatformVerifySettings.CreateDefault();
        var guid = Guid.NewGuid().ToString();
        var input = $"X-Correlation-Id: {guid} ts=2024-01-01T00:00:00Z id={Guid.NewGuid()}";

        var result = settings.Apply(input);

        result.Should().NotContain(guid);
        result.Should().NotMatchRegex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}");
        result.Should().Contain("«CorrelationId»");
        result.Should().Contain("«DateTimeOffset»");
    }

    [Fact]
    public void AddScrubber_CustomDelegate_IsApplied()
    {
        var settings = new PlatformVerifySettings().AddScrubber(s =>
            s.Replace("secret", "«REDACTED»", StringComparison.Ordinal)
        );
        var result = settings.Apply("value=secret");

        result.Should().Be("value=«REDACTED»");
    }

    [Fact]
    public void Scrubbers_AreAppliedInOrder()
    {
        var order = new List<int>();
        var settings = new PlatformVerifySettings()
            .AddScrubber(s =>
            {
                order.Add(1);
                return s;
            })
            .AddScrubber(s =>
            {
                order.Add(2);
                return s;
            });

        settings.Apply("x");

        order.Should().Equal(1, 2);
    }
}

// ── ApiResponseVerifier ───────────────────────────────────────────────────────

public sealed class ApiResponseVerifierTests
{
    [Fact]
    public async Task SnapshotAsync_NullResponse_Throws()
    {
        var act = async () => await ApiResponseVerifier.SnapshotAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SnapshotAsync_CapturesStatusCode()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(string.Empty) };

        var snapshot = await ApiResponseVerifier.SnapshotAsync(response, new PlatformVerifySettings());

        snapshot.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task SnapshotAsync_CapturesAndScrubsBody()
    {
        var guid = Guid.NewGuid().ToString();
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"{{\"id\":\"{guid}\"}}"),
        };
        var settings = new PlatformVerifySettings().AddGuidScrubber();

        var snapshot = await ApiResponseVerifier.SnapshotAsync(response, settings);

        snapshot.Body.Should().Contain("«Guid»");
        snapshot.Body.Should().NotContain(guid);
    }

    [Fact]
    public async Task SnapshotAsync_NotFoundStatusCode_IsPreserved()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("not found"),
        };

        var snapshot = await ApiResponseVerifier.SnapshotAsync(response, new PlatformVerifySettings());

        snapshot.StatusCode.Should().Be(404);
    }
}

// ── DomainEventVerifier ───────────────────────────────────────────────────────

public sealed class DomainEventVerifierTests
{
    private sealed record OrderCreatedEvent(Guid OrderId, DateTimeOffset OccurredAt, string CustomerId);

    [Fact]
    public void Snapshot_NullEvent_Throws()
    {
        var act = () => DomainEventVerifier.Snapshot<OrderCreatedEvent>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Snapshot_ProducesValidJson()
    {
        var evt = new OrderCreatedEvent(Guid.NewGuid(), DateTimeOffset.UtcNow, "customer-123");
        var settings = new PlatformVerifySettings();

        var result = DomainEventVerifier.Snapshot(evt, settings);

        result.Should().Contain("OrderId");
        result.Should().Contain("OccurredAt");
        result.Should().Contain("CustomerId");
    }

    [Fact]
    public void Snapshot_ScrubsGuidsAndTimestamps()
    {
        var evt = new OrderCreatedEvent(Guid.NewGuid(), DateTimeOffset.UtcNow, "customer-abc");
        var settings = new PlatformVerifySettings().AddGuidScrubber().AddDateTimeOffsetScrubber();

        var result = DomainEventVerifier.Snapshot(evt, settings);

        result.Should().Contain("«Guid»");
        result.Should().Contain("«DateTimeOffset»");
    }
}

// ── SqlQueryVerifier ──────────────────────────────────────────────────────────

public sealed class SqlQueryVerifierTests
{
    [Fact]
    public void Normalise_NullOrWhitespace_Throws()
    {
        var act = () => SqlQueryVerifier.Normalise("   ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Normalise_CollapsesInternalWhitespace()
    {
        var sql = "SELECT   *  FROM   Orders   WHERE  Id = 1";

        var result = SqlQueryVerifier.Normalise(sql);

        result.Should().Be("SELECT * FROM Orders WHERE Id = 1");
    }

    [Fact]
    public void Normalise_TrimsLeadingAndTrailingWhitespace()
    {
        var sql = "  SELECT 1  ";

        var result = SqlQueryVerifier.Normalise(sql);

        result.Should().Be("SELECT 1");
    }

    [Fact]
    public void Normalise_ReplacesNewlinesWithSpace()
    {
        var sql = "SELECT *\nFROM Orders\nWHERE Id = 1";

        var result = SqlQueryVerifier.Normalise(sql);

        result.Should().Be("SELECT * FROM Orders WHERE Id = 1");
    }

    [Fact]
    public void Normalise_ReplacesTabsWithSpace()
    {
        var sql = "SELECT\t*\tFROM\tOrders";

        var result = SqlQueryVerifier.Normalise(sql);

        result.Should().Be("SELECT * FROM Orders");
    }
}
