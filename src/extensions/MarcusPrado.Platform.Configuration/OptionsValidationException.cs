namespace MarcusPrado.Platform.Configuration;

/// <summary>
/// Exception thrown when options validation fails before a configuration change is applied.
/// </summary>
public sealed class OptionsValidationException : Exception
{
    /// <summary>
    /// Gets the name of the options type that failed validation.
    /// </summary>
    public string OptionsTypeName { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="OptionsValidationException"/>.
    /// </summary>
    /// <param name="optionsTypeName">The name of the options type.</param>
    /// <param name="message">A message describing the validation failure.</param>
    public OptionsValidationException(string optionsTypeName, string message)
        : base(message)
    {
        OptionsTypeName = optionsTypeName ?? throw new ArgumentNullException(nameof(optionsTypeName));
    }

    /// <summary>
    /// Initializes a new instance of <see cref="OptionsValidationException"/> with an inner exception.
    /// </summary>
    /// <param name="optionsTypeName">The name of the options type.</param>
    /// <param name="message">A message describing the validation failure.</param>
    /// <param name="innerException">The exception that caused this validation failure.</param>
    public OptionsValidationException(string optionsTypeName, string message, Exception innerException)
        : base(message, innerException)
    {
        OptionsTypeName = optionsTypeName ?? throw new ArgumentNullException(nameof(optionsTypeName));
    }
}
