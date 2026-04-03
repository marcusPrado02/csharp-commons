using System.Reflection;
using MarcusPrado.Platform.Abstractions.Email;
using MarcusPrado.Platform.MailKit.Options;

namespace MarcusPrado.Platform.MailKit.Email;

/// <summary>
/// File-based template renderer. Loads <c>{TemplateDirectory}/{templateName}.html</c>
/// and replaces <c>{{PropertyName}}</c> tokens with public property values from the <c>model</c> argument.
/// </summary>
public sealed class SimpleTemplateRenderer : IEmailTemplateRenderer
{
    private readonly MailKitOptions _options;

    /// <summary>Initializes a new instance of <see cref="SimpleTemplateRenderer"/>.</summary>
    public SimpleTemplateRenderer(MailKitOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <inheritdoc />
    public Task<string> RenderAsync(
        string templateName, object model, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateName);
        ArgumentNullException.ThrowIfNull(model);

        var path = Path.Combine(_options.TemplateDirectory, $"{templateName}.html");

        if (!File.Exists(path))
            throw new FileNotFoundException($"Email template '{templateName}' not found at '{path}'.");

        var template = File.ReadAllText(path);
        var rendered = ApplyTokens(template, model);
        return Task.FromResult(rendered);
    }

    private static string ApplyTokens(string template, object model)
    {
        var properties = model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            var value = prop.GetValue(model)?.ToString() ?? string.Empty;
            template = template.Replace($"{{{{{prop.Name}}}}}", value, StringComparison.Ordinal);
        }

        return template;
    }
}
