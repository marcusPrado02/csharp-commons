using MarcusPrado.Platform.Abstractions.Documents;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;

namespace MarcusPrado.Platform.Pdf.Extensions;

/// <summary>Extension methods to register QuestPDF document generation services.</summary>
public static class PdfExtensions
{
    /// <summary>
    /// Registers <see cref="IPdfGenerator"/> backed by QuestPDF.
    /// Automatically sets the community license.
    /// </summary>
    public static IServiceCollection AddPlatformPdf(
        this IServiceCollection services,
        Action<QuestPdfTemplateRegistry>? configure = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        QuestPDF.Settings.License = LicenseType.Community;

        var registry = new QuestPdfTemplateRegistry();
        configure?.Invoke(registry);

        services.AddSingleton(registry);
        services.AddSingleton<IPdfGenerator, QuestPdfGenerator>();

        return services;
    }
}
