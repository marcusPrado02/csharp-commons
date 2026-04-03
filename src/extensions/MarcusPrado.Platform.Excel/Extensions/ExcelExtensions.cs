using MarcusPrado.Platform.Abstractions.Documents;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Excel.Extensions;

/// <summary>Extension methods to register ClosedXML Excel services.</summary>
public static class ExcelExtensions
{
    /// <summary>
    /// Registers <see cref="IExcelWriter"/> and <see cref="IExcelReader"/>
    /// backed by ClosedXML.
    /// </summary>
    public static IServiceCollection AddPlatformExcel(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IExcelWriter, ClosedXmlExcelWriter>();
        services.AddSingleton<IExcelReader, ClosedXmlExcelReader>();

        return services;
    }
}
