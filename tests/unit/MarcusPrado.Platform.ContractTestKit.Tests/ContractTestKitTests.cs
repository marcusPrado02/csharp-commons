using System.Text.Json;
using FluentAssertions;
using MarcusPrado.Platform.ContractTestKit.Async;
using MarcusPrado.Platform.ContractTestKit.Pact;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MarcusPrado.Platform.ContractTestKit.Tests;

// ── ContractVerificationResult ────────────────────────────────────────────────

public sealed class ContractVerificationResultTests
{
    [Fact]
    public void Success_Record_HasCorrectValues()
    {
        var result = new ContractVerificationResult("get-user", true, null);

        result.Interaction.Should().Be("get-user");
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Failure_Record_HasErrorMessage()
    {
        var result = new ContractVerificationResult("get-user", false, "Expected 200 but got 404");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("404");
    }
}

// ── EventContractEnvelope ─────────────────────────────────────────────────────

public sealed class EventContractEnvelopeTests
{
    [Fact]
    public void Properties_AreCorrect()
    {
        using var doc = JsonDocument.Parse("{\"id\":\"abc\"}");
        var envelope = new EventContractEnvelope("OrderPlaced", "order-service", doc.RootElement);

        envelope.EventType.Should().Be("OrderPlaced");
        envelope.ProducerId.Should().Be("order-service");
        envelope.Payload.GetProperty("id").GetString().Should().Be("abc");
    }
}

// ── AsyncContractVerifier ─────────────────────────────────────────────────────

public sealed class AsyncContractVerifierTests
{
    [Fact]
    public void Verify_ValidPayload_ReturnsSuccess()
    {
        using var payloadDoc = JsonDocument.Parse("{\"orderId\":\"123\",\"amount\":99.99}");
        var envelope = new EventContractEnvelope("OrderPlaced", "svc", payloadDoc.RootElement);
        using var schemaDoc = JsonDocument.Parse("{\"required\":[\"orderId\",\"amount\"]}");

        var result = AsyncContractVerifier.Verify(envelope, schemaDoc);

        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Verify_MissingRequiredProperty_ReturnsFailure()
    {
        using var payloadDoc = JsonDocument.Parse("{\"orderId\":\"123\"}");
        var envelope = new EventContractEnvelope("OrderPlaced", "svc", payloadDoc.RootElement);
        using var schemaDoc = JsonDocument.Parse("{\"required\":[\"orderId\",\"amount\"]}");

        var result = AsyncContractVerifier.Verify(envelope, schemaDoc);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("amount");
    }

    [Fact]
    public void Verify_NullEnvelope_Throws()
    {
        using var schemaDoc = JsonDocument.Parse("{}");
        var act = () => AsyncContractVerifier.Verify(null!, schemaDoc);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Verify_NullSchema_Throws()
    {
        using var payloadDoc = JsonDocument.Parse("{}");
        var envelope = new EventContractEnvelope("E", "svc", payloadDoc.RootElement);
        var act = () => AsyncContractVerifier.Verify(envelope, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Verify_NonObjectPayload_ReturnsFailure()
    {
        using var payloadDoc = JsonDocument.Parse("\"not-an-object\"");
        var envelope = new EventContractEnvelope("OrderPlaced", "svc", payloadDoc.RootElement);
        using var schemaDoc = JsonDocument.Parse("{\"required\":[]}");

        var result = AsyncContractVerifier.Verify(envelope, schemaDoc);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("JSON object");
    }

    [Fact]
    public void Verify_TypeMismatch_ReturnsFailure()
    {
        using var payloadDoc = JsonDocument.Parse("{\"count\":\"not-a-number\"}");
        var envelope = new EventContractEnvelope("Stats", "svc", payloadDoc.RootElement);
        using var schemaDoc = JsonDocument.Parse(
            "{\"required\":[\"count\"],\"properties\":{\"count\":{\"type\":\"number\"}}}"
        );

        var result = AsyncContractVerifier.Verify(envelope, schemaDoc);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("count");
    }

    [Fact]
    public void Verify_CorrectType_ReturnsSuccess()
    {
        using var payloadDoc = JsonDocument.Parse("{\"count\":42}");
        var envelope = new EventContractEnvelope("Stats", "svc", payloadDoc.RootElement);
        using var schemaDoc = JsonDocument.Parse(
            "{\"required\":[\"count\"],\"properties\":{\"count\":{\"type\":\"number\"}}}"
        );

        var result = AsyncContractVerifier.Verify(envelope, schemaDoc);

        result.Success.Should().BeTrue();
    }
}

// ── PactVerifier ──────────────────────────────────────────────────────────────

public sealed class PactVerifierTests : IDisposable
{
    private readonly TestServer _server;

    public PactVerifierTests()
    {
        _server = new TestServer(
            new WebHostBuilder()
                .ConfigureServices(s => s.AddRouting())
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(e => e.MapGet("/ping", () => Results.Ok(new { pong = true })));
                })
        );
    }

    public void Dispose() => _server.Dispose();

    [Fact]
    public async Task VerifyAsync_FileNotFound_ThrowsFileNotFoundException()
    {
        using var pactVerifier = BuildPactVerifier();

        var act = async () => await pactVerifier.VerifyAsync("/nonexistent/pact.json");
        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task VerifyAsync_ValidPact_ReturnsSuccessResult()
    {
        using var pactFile = CreateTempPact("GET", "/ping", 200);
        using var pactVerifier = BuildPactVerifier();

        var results = await pactVerifier.VerifyAsync(pactFile.FullName);

        results.Should().HaveCount(1);
        results[0].Success.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyAsync_WrongStatus_ReturnsFailureResult()
    {
        using var pactFile = CreateTempPact("GET", "/ping", 404);
        using var pactVerifier = BuildPactVerifier();

        var results = await pactVerifier.VerifyAsync(pactFile.FullName);

        results.Should().HaveCount(1);
        results[0].Success.Should().BeFalse();
        results[0].ErrorMessage.Should().Contain("404");
    }

    [Fact]
    public async Task VerifyAsync_NoInteractionsArray_ThrowsInvalidOperationException()
    {
        var pactPath = Path.GetTempFileName();
        await File.WriteAllTextAsync(pactPath, "{\"consumer\":\"a\",\"provider\":\"b\"}");

        using var pactVerifier = BuildPactVerifier();
        var act = async () => await pactVerifier.VerifyAsync(pactPath);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*interactions*");

        File.Delete(pactPath);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Creates a <see cref="PactVerifier{T}"/> backed by the in-process TestServer.</summary>
    private PactVerifier<object> BuildPactVerifier() => new(_server.CreateClient());

    private static TempPactFile CreateTempPact(string method, string path, int expectedStatus)
    {
        var pact = new
        {
            consumer = new { name = "test-consumer" },
            provider = new { name = "test-provider" },
            interactions = new[]
            {
                new
                {
                    description = $"{method} {path} - {expectedStatus}",
                    request = new { method, path },
                    response = new { status = expectedStatus },
                },
            },
        };

        var filePath = Path.GetTempFileName();
        File.WriteAllText(filePath, JsonSerializer.Serialize(pact));
        return new TempPactFile(filePath);
    }

    private sealed class TempPactFile : IDisposable
    {
        public TempPactFile(string fullName) => FullName = fullName;

        public string FullName { get; }

        public void Dispose()
        {
            if (File.Exists(FullName))
                File.Delete(FullName);
        }
    }
}
