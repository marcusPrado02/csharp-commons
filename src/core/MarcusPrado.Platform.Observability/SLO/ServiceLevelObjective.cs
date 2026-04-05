namespace MarcusPrado.Platform.Observability.SLO;

/// <summary>
/// Defines a Service Level Objective with a target availability rate and measurement window.
/// </summary>
/// <param name="Name">The human-readable name of the SLO (e.g. "API Availability").</param>
/// <param name="Target">The target availability rate expressed as a fraction between 0 and 1 (e.g. 0.999 for 99.9%).</param>
/// <param name="Window">The rolling time window over which the SLO is measured.</param>
public sealed record ServiceLevelObjective(string Name, double Target, TimeSpan Window);
