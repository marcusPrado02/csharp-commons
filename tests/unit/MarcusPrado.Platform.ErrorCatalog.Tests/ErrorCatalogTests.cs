using System.Reflection;
using FluentAssertions;
using MarcusPrado.Platform.Abstractions.Errors;
using MarcusPrado.Platform.ErrorCatalog;
using Xunit;

namespace MarcusPrado.Platform.ErrorCatalog.Tests;

/// <summary>
/// Unit tests for <see cref="ErrorCatalog"/>, <see cref="IErrorTranslator"/>,
/// <see cref="LocalizedErrorTranslator"/>, and <see cref="ErrorDocumentationGenerator"/>.
/// </summary>
public sealed class ErrorCatalogTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static IReadOnlyList<Error> AllCatalogErrors()
    {
        var errors = new List<Error>();
        CollectErrors(typeof(ErrorCatalog), errors);
        return errors;
    }

    private static void CollectErrors(Type type, List<Error> target)
    {
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static)
                     .Where(f => f.FieldType == typeof(Error) && f.IsInitOnly))
        {
            target.Add((Error)field.GetValue(null)!);
        }

        foreach (var nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
        {
            CollectErrors(nested, target);
        }
    }

    // ── Test 1: All errors have non-null, non-empty Code and Message ─────────

    [Fact]
    public void AllErrorsInCatalog_HaveNonEmptyCodeAndMessage()
    {
        var errors = AllCatalogErrors();

        errors.Should().NotBeEmpty("the catalog must contain at least one error");

        foreach (var error in errors)
        {
            error.Code.Should().NotBeNullOrWhiteSpace(
                because: $"error declared in ErrorCatalog must have a non-empty Code (offender message: '{error.Message}')");

            error.Message.Should().NotBeNullOrWhiteSpace(
                because: $"error with code '{error.Code}' must have a non-empty Message");
        }
    }

    // ── Test 2: Payment.NotFound has correct code ────────────────────────────

    [Fact]
    public void PaymentNotFound_HasCorrectCode()
    {
        ErrorCatalog.Payment.NotFound.Code.Should().Be("PAYMENT_001");
    }

    // ── Test 3: User.NotFound has correct code ───────────────────────────────

    [Fact]
    public void UserNotFound_HasCorrectCode()
    {
        ErrorCatalog.User.NotFound.Code.Should().Be("USER_001");
    }

    // ── Test 4: Translator returns Error.Message when no translation found ───

    [Fact]
    public void LocalizedErrorTranslator_ReturnsFallbackMessage_WhenNoTranslationRegistered()
    {
        var translator = new LocalizedErrorTranslator();

        var result = translator.Translate(ErrorCatalog.Payment.NotFound, "pt-BR");

        result.Should().Be(ErrorCatalog.Payment.NotFound.Message);
    }

    // ── Test 5: Translator returns registered translation ────────────────────

    [Fact]
    public void LocalizedErrorTranslator_ReturnsTranslatedMessage_WhenTranslationRegistered()
    {
        var translator = new LocalizedErrorTranslator();
        translator.Register("PAYMENT_001", "pt-BR", "Pagamento não encontrado.");

        var result = translator.Translate(ErrorCatalog.Payment.NotFound, "pt-BR");

        result.Should().Be("Pagamento não encontrado.");
    }

    // ── Test 6: GenerateMarkdownTable returns markdown with header row ────────

    [Fact]
    public void GenerateMarkdownTable_ReturnsMarkdownWithHeaderRow()
    {
        var table = ErrorDocumentationGenerator.GenerateMarkdownTable();

        table.Should().Contain("| Code | Type | Message |");
        table.Should().Contain("|------|------|---------|");
    }

    // ── Test 7: GenerateMarkdownTable includes all error codes ───────────────

    [Fact]
    public void GenerateMarkdownTable_IncludesAllErrorCodes()
    {
        var table = ErrorDocumentationGenerator.GenerateMarkdownTable();
        var errors = AllCatalogErrors();

        foreach (var error in errors)
        {
            table.Should().Contain(error.Code,
                because: $"the markdown table must include every error code, missing: '{error.Code}'");
        }
    }

    // ── Test 8: All error codes are unique ───────────────────────────────────

    [Fact]
    public void AllErrorsInCatalog_HaveUniqueCodes()
    {
        var errors = AllCatalogErrors();
        var codes = errors.Select(e => e.Code).ToList();

        codes.Should().OnlyHaveUniqueItems(because: "no two catalog errors may share the same Code");
    }

    // ── Extra: Order.InvalidState has correct code ───────────────────────────

    [Fact]
    public void OrderInvalidState_HasCorrectCode()
    {
        ErrorCatalog.Order.InvalidState.Code.Should().Be("ORDER_002");
    }

    // ── Extra: Translator handles null culture gracefully ────────────────────

    [Fact]
    public void LocalizedErrorTranslator_HandlesNullCulture_WithFallback()
    {
        var translator = new LocalizedErrorTranslator();

        var result = translator.Translate(ErrorCatalog.User.AlreadyExists, culture: null);

        result.Should().Be(ErrorCatalog.User.AlreadyExists.Message);
    }
}
