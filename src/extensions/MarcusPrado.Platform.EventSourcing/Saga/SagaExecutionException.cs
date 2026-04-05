namespace MarcusPrado.Platform.EventSourcing.Saga;

/// <summary>
/// Thrown when a saga step fails during orchestration.
/// </summary>
public sealed class SagaExecutionException : Exception
{
    /// <summary>Gets the name of the step that caused the failure.</summary>
    public string FailedStepName { get; }

    /// <summary>
    /// Initialises a new <see cref="SagaExecutionException"/>.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="failedStepName">The name of the failed step.</param>
    /// <param name="innerException">The original exception from the step.</param>
    public SagaExecutionException(string message, string failedStepName, Exception innerException)
        : base(message, innerException)
    {
        FailedStepName = failedStepName;
    }
}
