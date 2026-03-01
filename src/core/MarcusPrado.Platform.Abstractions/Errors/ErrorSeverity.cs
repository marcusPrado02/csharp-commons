namespace MarcusPrado.Platform.Abstractions.Errors;

/// <summary>
/// Indicates the operational severity of an <see cref="Error"/>,
/// driving log levels, SLO burn-rate alerts and on-call escalations.
/// </summary>
/// <remarks>
/// Recommended log-level mapping:
/// <list type="table">
///   <item><term><see cref="Info"/></term><description><c>ILogger.LogInformation</c></description></item>
///   <item><term><see cref="Warning"/></term><description><c>ILogger.LogWarning</c></description></item>
///   <item><term><see cref="Error"/></term><description><c>ILogger.LogError</c></description></item>
///   <item><term><see cref="Critical"/></term><description><c>ILogger.LogCritical</c> + PagerDuty/OpsGenie alert</description></item>
/// </list>
/// </remarks>
public enum ErrorSeverity
{
    /// <summary>
    /// Informational — represents a normal, expected branch in the business flow
    /// (e.g. <c>NOT_FOUND</c>, <c>ALREADY_EXISTS</c>).
    /// </summary>
    Info,

    /// <summary>
    /// Degraded functionality or an unexpected but non-critical client error.
    /// The system continues operating; no immediate on-call action required.
    /// </summary>
    Warning,

    /// <summary>
    /// A significant error that must be investigated and corrected,
    /// though the system can continue serving other requests.
    /// </summary>
    Error,

    /// <summary>
    /// A severe error that may cause data loss, cascading failures or
    /// system instability. Triggers immediate on-call escalation.
    /// </summary>
    Critical,
}
