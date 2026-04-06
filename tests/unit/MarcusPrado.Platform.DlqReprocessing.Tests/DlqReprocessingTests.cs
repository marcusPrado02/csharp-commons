using FluentAssertions;
using MarcusPrado.Platform.DlqReprocessing.Endpoints;
using MarcusPrado.Platform.DlqReprocessing.Extensions;
using MarcusPrado.Platform.DlqReprocessing.Metrics;
using MarcusPrado.Platform.DlqReprocessing.Models;
using MarcusPrado.Platform.DlqReprocessing.Options;
using MarcusPrado.Platform.DlqReprocessing.Store;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace MarcusPrado.Platform.DlqReprocessing.Tests;

public sealed class InMemoryDlqStoreTests
{
    private static DlqMessage MakeMessage(string id = "msg-1", string topic = "orders") =>
        new(id, topic, "{}", "TimeoutException", 1, DateTimeOffset.UtcNow, null);

    [Fact]
    public async Task InMemoryDlqStore_AddAndGet_ReturnsMessages()
    {
        var store = new InMemoryDlqStore();
        var msg = MakeMessage();

        await store.AddAsync(msg);

        var result = await store.GetAsync("orders");
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("msg-1");
    }

    [Fact]
    public async Task InMemoryDlqStore_GetByIdAsync_ReturnsCorrectMessage()
    {
        var store = new InMemoryDlqStore();
        var msg = MakeMessage("abc-123");
        await store.AddAsync(msg);

        var found = await store.GetByIdAsync("orders", "abc-123");

        found.Should().NotBeNull();
        found!.Id.Should().Be("abc-123");
    }

    [Fact]
    public async Task InMemoryDlqStore_GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var store = new InMemoryDlqStore();

        var found = await store.GetByIdAsync("orders", "nonexistent");

        found.Should().BeNull();
    }

    [Fact]
    public async Task InMemoryDlqStore_RequeueAsync_MovesMessage()
    {
        var store = new InMemoryDlqStore();
        var msg = MakeMessage();
        await store.AddAsync(msg);

        await store.RequeueAsync("orders", "msg-1");

        var result = await store.GetAsync("orders");
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task InMemoryDlqStore_DeleteAsync_RemovesMessage()
    {
        var store = new InMemoryDlqStore();
        var msg = MakeMessage();
        await store.AddAsync(msg);

        await store.DeleteAsync("orders", "msg-1");

        var result = await store.GetAsync("orders");
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task InMemoryDlqStore_MultipleTopics_AreIsolated()
    {
        var store = new InMemoryDlqStore();
        await store.AddAsync(MakeMessage("m1", "orders"));
        await store.AddAsync(MakeMessage("m2", "payments"));

        var orders = await store.GetAsync("orders");
        var payments = await store.GetAsync("payments");

        orders.Should().HaveCount(1);
        payments.Should().HaveCount(1);
    }
}

public sealed class DlqOptionsTests
{
    [Fact]
    public void DlqOptions_Defaults_AreCorrect()
    {
        var options = new DlqOptions();

        options.PollingIntervalSeconds.Should().Be(30);
        options.AlertThreshold.Should().Be(100);
        options.Topics.Should().BeEmpty();
    }
}

public sealed class OtelDlqMetricsTests
{
    [Fact]
    public void OtelDlqMetrics_RecordDepth_DoesNotThrow()
    {
        using var metrics = new OtelDlqMetrics();

        var act = () => metrics.RecordDepth("orders", 42);

        act.Should().NotThrow();
    }

    [Fact]
    public void OtelDlqMetrics_RecordReprocessed_DoesNotThrow()
    {
        using var metrics = new OtelDlqMetrics();

        var act = () => metrics.RecordReprocessed("orders");

        act.Should().NotThrow();
    }

    [Fact]
    public void OtelDlqMetrics_RecordDeleted_DoesNotThrow()
    {
        using var metrics = new OtelDlqMetrics();

        var act = () => metrics.RecordDeleted("payments");

        act.Should().NotThrow();
    }
}

public sealed class DlqExtensionsTests
{
    [Fact]
    public void AddPlatformDlqReprocessing_RegistersStore()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPlatformDlqReprocessing();

        using var provider = services.BuildServiceProvider();
        var store = provider.GetService<IDlqStore>();

        store.Should().NotBeNull();
        store.Should().BeOfType<InMemoryDlqStore>();
    }

    [Fact]
    public void AddPlatformDlqReprocessing_RegistersMetrics()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPlatformDlqReprocessing();

        using var provider = services.BuildServiceProvider();
        var metrics = provider.GetService<IDlqMetrics>();

        metrics.Should().NotBeNull();
        metrics.Should().BeOfType<OtelDlqMetrics>();
    }
}

public sealed class DlqEndpointsTests
{
    [Fact]
    public async Task DlqEndpoints_GetMessages_ReturnsOk()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddLogging();
                    services.AddPlatformDlqReprocessing();
                });
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => endpoints.MapDlqEndpoints());
                });
            })
            .StartAsync();

        // Seed a message so the response body is non-empty.
        var store = host.Services.GetRequiredService<IDlqStore>();
        await store.AddAsync(new DlqMessage("id-1", "orders", "{}", "timeout", 1, DateTimeOffset.UtcNow, null));

        var client = host.GetTestClient();
        var response = await client.GetAsync("/dlq/orders");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("id-1");
    }

    [Fact]
    public async Task DlqEndpoints_ReprocessMessage_Returns200()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddLogging();
                    services.AddPlatformDlqReprocessing();
                });
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => endpoints.MapDlqEndpoints());
                });
            })
            .StartAsync();

        var store = host.Services.GetRequiredService<IDlqStore>();
        await store.AddAsync(new DlqMessage("id-2", "orders", "{}", "timeout", 1, DateTimeOffset.UtcNow, null));

        var client = host.GetTestClient();
        var response = await client.PostAsync("/dlq/orders/reprocess/id-2", content: null);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task DlqEndpoints_ReprocessMessage_Returns404_WhenNotFound()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddLogging();
                    services.AddPlatformDlqReprocessing();
                });
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => endpoints.MapDlqEndpoints());
                });
            })
            .StartAsync();

        var client = host.GetTestClient();
        var response = await client.PostAsync("/dlq/orders/reprocess/ghost", content: null);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DlqEndpoints_DeleteMessage_Returns204()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddLogging();
                    services.AddPlatformDlqReprocessing();
                });
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => endpoints.MapDlqEndpoints());
                });
            })
            .StartAsync();

        var store = host.Services.GetRequiredService<IDlqStore>();
        await store.AddAsync(new DlqMessage("id-3", "orders", "{}", "timeout", 1, DateTimeOffset.UtcNow, null));

        var client = host.GetTestClient();
        var response = await client.DeleteAsync("/dlq/orders/id-3");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }
}
