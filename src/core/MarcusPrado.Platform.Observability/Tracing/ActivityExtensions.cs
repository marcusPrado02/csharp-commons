using System.Diagnostics;

namespace MarcusPrado.Platform.Observability.Tracing;

/// <summary>
/// Extension methods for <see cref="Activity"/> that set platform-specific
/// tags (e.g. tenant, user, correlation, error status).
/// </summary>
public static class ActivityExtensions
{
    /// <summary>Sets the <c>tenant.id</c> tag on the activity.</summary>
    public static Activity? SetTenantId(this Activity? activity, string? tenantId)
    {
        activity?.SetTag("tenant.id", tenantId);
        return activity;
    }

    /// <summary>Sets the <c>user.id</c> tag on the activity.</summary>
    public static Activity? SetUserId(this Activity? activity, string? userId)
    {
        activity?.SetTag("user.id", userId);
        return activity;
    }

    /// <summary>Sets the <c>correlation.id</c> tag on the activity.</summary>
    public static Activity? SetCorrelationId(this Activity? activity, string? correlationId)
    {
        activity?.SetTag("correlation.id", correlationId);
        return activity;
    }

    /// <summary>
    /// Marks the activity as failed and records the exception details.
    /// Sets <c>otel.status_code = ERROR</c> and <c>exception.message</c>.
    /// </summary>
    public static Activity? SetErrorStatus(this Activity? activity, Exception exception)
    {
        if (activity is null)
        {
            return null;
        }

        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity.SetTag("exception.type", exception.GetType().FullName);
        activity.SetTag("exception.message", exception.Message);
        return activity;
    }

    /// <summary>
    /// Marks the activity as failed with a plain message.
    /// </summary>
    public static Activity? SetErrorStatus(this Activity? activity, string errorMessage)
    {
        activity?.SetStatus(ActivityStatusCode.Error, errorMessage);
        return activity;
    }
}
