namespace MarcusPrado.Platform.OpenTelemetry.Conventions;

/// <summary>Semantic attribute names used on platform spans.</summary>
public static class PlatformSpanAttributes
{
    /// <summary>The tenant identifier attribute name.</summary>
    public const string TenantId = "tenant.id";

    /// <summary>The correlation identifier attribute name.</summary>
    public const string CorrelationId = "correlation.id";

    /// <summary>The user identifier attribute name.</summary>
    public const string UserId = "user.id";

    /// <summary>The CQRS command name attribute.</summary>
    public const string CommandName = "command.name";

    /// <summary>The domain event name attribute.</summary>
    public const string EventName = "event.name";

    /// <summary>The deployment environment attribute.</summary>
    public const string Environment = "deployment.environment";
}
