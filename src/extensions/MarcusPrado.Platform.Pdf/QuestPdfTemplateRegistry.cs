using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace MarcusPrado.Platform.Pdf;

/// <summary>Registry that maps template names to QuestPDF page composer delegates.</summary>
public sealed class QuestPdfTemplateRegistry
{
    private readonly Dictionary<string, Action<PageDescriptor, object>> _composers = new(
        StringComparer.OrdinalIgnoreCase
    );

    /// <summary>Registers a named template composer.</summary>
    public QuestPdfTemplateRegistry Register(string templateName, Action<PageDescriptor, object> composer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateName);
        ArgumentNullException.ThrowIfNull(composer);

        _composers[templateName] = composer;
        return this;
    }

    /// <summary>Attempts to find a registered composer for the given template name.</summary>
    public bool TryGetComposer(string templateName, out Action<PageDescriptor, object>? composer) =>
        _composers.TryGetValue(templateName, out composer);
}
