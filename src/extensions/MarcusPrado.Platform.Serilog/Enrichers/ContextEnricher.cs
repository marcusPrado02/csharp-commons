using Serilog.Core;
using Serilog.Events;

namespace MarcusPrado.Platform.Serilog.Enrichers;

/// <summary>
/// Enriches log events with platform context: ApplicationName, Environment, MachineName.
/// </summary>
public sealed class ContextEnricher : ILogEventEnricher
{
    private readonly string _applicationName;
    private readonly string _environment;

    /// <summary>Initialises the enricher with platform context values.</summary>
    public ContextEnricher(string applicationName, string environment)
    {
        _applicationName = applicationName;
        _environment = environment;
    }

    /// <inheritdoc/>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ApplicationName", _applicationName));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Environment", _environment));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("MachineName", System.Environment.MachineName));
    }
}
