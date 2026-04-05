using System.Globalization;
using System.Net;
using MarcusPrado.Platform.AspNetCore.Localization;
using MarcusPrado.Platform.AspNetCore.Tests.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace MarcusPrado.Platform.AspNetCore.Tests;

/// <summary>
/// Tests for the platform localization infrastructure:
/// <see cref="AcceptLanguageMiddleware"/>, <see cref="LocalizedErrorTranslator"/>,
/// <see cref="ValidationMessageLocalizer"/>, and <see cref="LocalizationExtensions"/>.
/// </summary>
public sealed class LocalizationTests
{
    // ── AcceptLanguageMiddleware ───────────────────────────────────────────────

    [Fact]
    public async Task Middleware_SetsCulture_WhenValidAcceptLanguageHeader_IsProvided()
    {
        using var server = CreateLocalizationServer(capturedCulture: out var cultureHolder);
        using var client = server.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/culture");
        request.Headers.Add("Accept-Language", "pt-BR");

        await client.SendAsync(request);

        cultureHolder.Value.Should().Be("pt-BR",
            because: "middleware should resolve pt-BR from the Accept-Language header");
    }

    [Fact]
    public async Task Middleware_UsesDefaultCulture_WhenAcceptLanguageHeader_IsAbsent()
    {
        using var server = CreateLocalizationServer(capturedCulture: out var cultureHolder);
        using var client = server.CreateClient();

        await client.GetAsync("/culture");

        cultureHolder.Value.Should().Be("en-US",
            because: "middleware should fall back to the configured default culture");
    }

    [Fact]
    public async Task Middleware_UsesDefaultCulture_WhenAcceptLanguageHeader_ContainsUnsupportedLocale()
    {
        using var server = CreateLocalizationServer(capturedCulture: out var cultureHolder);
        using var client = server.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/culture");
        request.Headers.Add("Accept-Language", "zh-CN,ja;q=0.9");

        await client.SendAsync(request);

        cultureHolder.Value.Should().Be("en-US",
            because: "middleware should fall back to default when no supported locale matches");
    }

    [Fact]
    public async Task Middleware_DoesNotThrow_WhenAcceptLanguageHeader_IsMalformed()
    {
        using var server = CreateLocalizationServer(capturedCulture: out _);
        using var client = server.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/culture");
        request.Headers.TryAddWithoutValidation("Accept-Language", ";;;invalid;;;");

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK,
            because: "malformed Accept-Language header must not cause a 500 error");
    }

    [Fact]
    public async Task Middleware_ResolvesHighestQualityLocale_WhenMultipleLocales_AreProvided()
    {
        using var server = CreateLocalizationServer(capturedCulture: out var cultureHolder);
        using var client = server.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/culture");
        // es-ES has lower quality than pt-BR
        request.Headers.Add("Accept-Language", "es-ES;q=0.7,pt-BR;q=0.9");

        await client.SendAsync(request);

        cultureHolder.Value.Should().Be("pt-BR",
            because: "middleware should choose the supported locale with the highest q-value");
    }

    [Fact]
    public async Task Middleware_MatchesByLanguage_WhenExactCultureNotSupported()
    {
        using var server = CreateLocalizationServer(capturedCulture: out var cultureHolder);
        using var client = server.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/culture");
        // "es" should match "es-ES"
        request.Headers.Add("Accept-Language", "es");

        await client.SendAsync(request);

        cultureHolder.Value.Should().Be("es-ES",
            because: "middleware should match 'es' to the supported 'es-ES' culture");
    }

    // ── LocalizedErrorTranslator ──────────────────────────────────────────────

    [Fact]
    public void Translator_ReturnsEnglishMessage_ForNotFoundError_InEnUs()
    {
        var error = Error.NotFound("RES.NOT_FOUND", "fallback message");
        var enUs = CultureInfo.GetCultureInfo("en-US");

        var message = LocalizedErrorTranslator.Translate(error, enUs);

        message.Should().Be("The requested resource was not found.",
            because: "en-US resource should contain the English not-found message");
    }

    [Fact]
    public void Translator_ReturnsPortugueseMessage_ForNotFoundError_InPtBr()
    {
        var error = Error.NotFound("RES.NOT_FOUND", "fallback message");
        var ptBr = CultureInfo.GetCultureInfo("pt-BR");

        var message = LocalizedErrorTranslator.Translate(error, ptBr);

        message.Should().Be("O recurso solicitado não foi encontrado.",
            because: "pt-BR resource should contain the Portuguese not-found message");
    }

    [Fact]
    public void Translator_ReturnsSpanishMessage_ForUnauthorizedError_InEsEs()
    {
        var error = Error.Unauthorized("AUTH.UNAUTH", "fallback message");
        var esEs = CultureInfo.GetCultureInfo("es-ES");

        var message = LocalizedErrorTranslator.Translate(error, esEs);

        message.Should().Be("No está autorizado para realizar esta acción.",
            because: "es-ES resource should contain the Spanish unauthorized message");
    }

    [Fact]
    public void Translator_ReturnsPortugueseMessage_ForValidationError_InPtBr()
    {
        var error = Error.Validation("VAL.REQUIRED", "fallback message");
        var ptBr = CultureInfo.GetCultureInfo("pt-BR");

        var message = LocalizedErrorTranslator.Translate(error, ptBr);

        message.Should().Be("Um ou mais erros de validação ocorreram.",
            because: "pt-BR resource should contain the Portuguese validation message");
    }

    // ── PlatformLocalizationOptions ───────────────────────────────────────────

    [Fact]
    public void Options_HaveCorrectDefaults()
    {
        var options = new PlatformLocalizationOptions();

        options.DefaultCulture.Should().Be("en-US",
            because: "default culture should be en-US");
        options.SupportedCultures.Should().BeEquivalentTo(
            ["en-US", "pt-BR", "es-ES"],
            because: "default supported cultures should include en-US, pt-BR, and es-ES");
    }

    // ── AddPlatformLocalization registration ──────────────────────────────────

    [Fact]
    public void AddPlatformLocalization_RegistersValidationMessageLocalizer()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPlatformLocalization();

        var provider = services.BuildServiceProvider();

        var localizer = provider.GetService<ValidationMessageLocalizer>();
        localizer.Should().NotBeNull(
            because: "AddPlatformLocalization should register ValidationMessageLocalizer");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Holder used to capture the resolved culture name from inside a test request handler.
    /// </summary>
    private sealed class CultureHolder
    {
        public string Value { get; set; } = string.Empty;
    }

    private static TestServer CreateLocalizationServer(out CultureHolder capturedCulture)
    {
        var holder = new CultureHolder();
        capturedCulture = holder;

        var builder = new WebHostBuilder()
            .UseEnvironment("Test")
            .ConfigureLogging(l => l.ClearProviders())
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddPlatformLocalization();
            })
            .Configure(app =>
            {
                app.UsePlatformLocalization();

                app.Run(async ctx =>
                {
                    if (ctx.Request.Path == "/culture")
                    {
                        // Capture the culture that the middleware resolved.
                        holder.Value = CultureInfo.CurrentUICulture.Name;
                        ctx.Response.StatusCode = 200;
                        await ctx.Response.WriteAsync(holder.Value);
                    }
                    else
                    {
                        ctx.Response.StatusCode = 404;
                    }
                });
            });

        return new TestServer(builder);
    }
}
